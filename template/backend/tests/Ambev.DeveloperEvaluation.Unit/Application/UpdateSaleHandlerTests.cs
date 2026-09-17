using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
/// Tests cover success/failure handling as well as the quantity discount tiers
/// that must be recomputed by the handler on every update.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _eventPublisher);

        // Mirrors UpdateSaleProfile's CreateMap<SaleProductDto, SaleProduct>(): the handler
        // maps each product line individually, not the command as a whole.
        _mapper.Map<SaleProduct>(Arg.Any<SaleProductDto>()).Returns(callInfo =>
        {
            var dto = callInfo.Arg<SaleProductDto>();
            return new SaleProduct
            {
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                UnitPrice = dto.UnitPrice,
                Quantity = dto.Quantity
            };
        });
    }

    private static Sale GenerateExistingSale(Guid id)
    {
        return new Sale
        {
            Id = id,
            SaleNumber = "SALE-0001",
            CustomerId = Guid.NewGuid(),
            CustomerName = "Old Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Old Branch",
            SaleProducts = new List<SaleProduct>()
        };
    }

    /// <summary>
    /// Tests that a valid sale update request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale data When updating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        var existingSale = GenerateExistingSale(command.Id);

        _saleRepository.GetAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
        _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>()).Returns(new UpdateSaleResult { Id = command.Id });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(command.Id);
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that updating a sale that does not exist throws a not-found exception.
    /// </summary>
    [Fact(DisplayName = "Given a sale that does not exist When updating sale Then throws key not found exception")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.GetAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Tests that an invalid sale update request throws a validation exception.
    /// </summary>
    [Fact(DisplayName = "Given invalid sale data When updating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var command = new UpdateSaleCommand(); // Empty command will fail validation (no id, no customer)

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that a sale cannot be updated to contain more than one product line for the same
    /// ProductId: the request must be rejected by validation before the existing sale is loaded
    /// or persisted.
    /// </summary>
    [Fact(DisplayName = "Given two product lines with the same ProductId When updating sale Then throws validation exception")]
    public async Task Handle_DuplicateProductId_ThrowsValidationException()
    {
        // Arrange: same ProductId used by two different lines
        var command = UpdateSaleHandlerTestData.GenerateCommandWithDuplicateProductId(
            firstQuantity: 3, firstUnitPrice: 1000,
            secondQuantity: 10, secondUnitPrice: 1000);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that a SaleModifiedEvent is published with the sale's recomputed total after update.
    /// </summary>
    [Fact(DisplayName = "Given an updated sale When handling Then publishes a SaleModifiedEvent")]
    public async Task Handle_ValidRequest_PublishesSaleModifiedEvent()
    {
        // Arrange
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        command.Products = new[]
        {
            new SaleProductDto { ProductId = Guid.NewGuid(), ProductName = "Item", UnitPrice = 1000, Quantity = 10 }
        };
        var existingSale = GenerateExistingSale(command.Id);

        _saleRepository.GetAsync(command.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
        _mapper.Map<UpdateSaleResult>(Arg.Any<Sale>()).Returns(new UpdateSaleResult());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<SaleModifiedEvent>(e => e.SaleId == existingSale.Id && e.Total == 8000),
            Arg.Any<CancellationToken>());
    }
}
