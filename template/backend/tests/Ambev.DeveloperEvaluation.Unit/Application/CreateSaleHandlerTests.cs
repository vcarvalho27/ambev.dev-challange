using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
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
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
/// Tests cover success/failure handling as well as the quantity discount tiers
/// that must be computed by the handler rather than accepted from the request.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateSaleHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSaleHandlerTests"/> class.
    /// Sets up the test dependencies and creates fake data generators.
    /// </summary>
    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _eventPublisher);
    }

    /// <summary>
    /// Builds the Sale entity that the mapper would produce for the given command,
    /// mirroring the CreateSaleProfile mapping (Discount/TotalAmount are left at
    /// their default values, exactly like the real mapping profile, since they are
    /// only computed afterwards by the handler).
    /// </summary>
    private static Sale MapToSale(CreateSaleCommand command)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = command.SaleNumber,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName,
            SaleProducts = command.Products.Select(p => new SaleProduct
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                Quantity = p.Quantity
            }).ToList()
        };
    }

    /// <summary>
    /// Tests that a valid sale creation request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = MapToSale(command);
        var result = new CreateSaleResult { Id = sale.Id };

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(result);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        // Act
        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        createSaleResult.Should().NotBeNull();
        createSaleResult.Id.Should().Be(sale.Id);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that an invalid sale creation request throws a validation exception.
    /// </summary>
    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var command = new CreateSaleCommand(); // Empty command will fail validation (no customer, no products)

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that a product line with a quantity above the 20 item maximum throws a validation exception,
    /// instead of being silently accepted.
    /// </summary>
    [Fact(DisplayName = "Given a product quantity above the maximum When creating sale Then throws validation exception")]
    public async Task Handle_QuantityAboveMaximum_ThrowsValidationException()
    {
        // Arrange
        var command = CreateSaleHandlerTestData.GenerateCommandWithSingleProduct(quantity: 21);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that no discount is applied for a product quantity below the minimum discount tier.
    /// </summary>
    [Fact(DisplayName = "Given a quantity below 4 items When creating sale Then no discount is applied")]
    public async Task Handle_QuantityBelowDiscountTier_AppliesNoDiscount()
    {
        // Arrange
        var command = CreateSaleHandlerTestData.GenerateCommandWithSingleProduct(quantity: 3, unitPrice: 1000);
        var sale = MapToSale(command);

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.SaleProducts.Single().Discount == 0
                              && s.SaleProducts.Single().TotalAmount == 3000
                              && s.Subtotal == 3000
                              && s.Discount == 0
                              && s.Total == 3000),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that a 10% discount is applied for a product quantity within the medium discount tier (4-9 items).
    /// </summary>
    [Fact(DisplayName = "Given a quantity between 4 and 9 items When creating sale Then a 10% discount is applied")]
    public async Task Handle_QuantityInMediumDiscountTier_AppliesTenPercentDiscount()
    {
        // Arrange
        var command = CreateSaleHandlerTestData.GenerateCommandWithSingleProduct(quantity: 4, unitPrice: 1000);
        var sale = MapToSale(command);

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.SaleProducts.Single().Discount == 10
                              && s.SaleProducts.Single().TotalAmount == 3600
                              && s.Subtotal == 4000
                              && s.Discount == 400
                              && s.Total == 3600),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that a 20% discount is applied for a product quantity within the high discount tier (10-20 items).
    /// </summary>
    [Fact(DisplayName = "Given a quantity between 10 and 20 items When creating sale Then a 20% discount is applied")]
    public async Task Handle_QuantityInHighDiscountTier_AppliesTwentyPercentDiscount()
    {
        // Arrange
        var command = CreateSaleHandlerTestData.GenerateCommandWithSingleProduct(quantity: 20, unitPrice: 1000);
        var sale = MapToSale(command);

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.SaleProducts.Single().Discount == 20
                              && s.SaleProducts.Single().TotalAmount == 16000
                              && s.Subtotal == 20000
                              && s.Discount == 4000
                              && s.Total == 16000),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that a sale cannot contain more than one product line for the same ProductId:
    /// the request must be rejected by validation before any pricing or persistence happens.
    /// </summary>
    [Fact(DisplayName = "Given two product lines with the same ProductId When creating sale Then throws validation exception")]
    public async Task Handle_DuplicateProductId_ThrowsValidationException()
    {
        // Arrange: same ProductId used by two different lines
        var command = CreateSaleHandlerTestData.GenerateCommandWithDuplicateProductId(
            firstQuantity: 3, firstUnitPrice: 1000,
            secondQuantity: 10, secondUnitPrice: 1000);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that a SaleCreatedEvent is published with the sale's computed total after creation.
    /// </summary>
    [Fact(DisplayName = "Given a created sale When handling Then publishes a SaleCreatedEvent")]
    public async Task Handle_ValidRequest_PublishesSaleCreatedEvent()
    {
        // Arrange
        var command = CreateSaleHandlerTestData.GenerateCommandWithSingleProduct(quantity: 10, unitPrice: 1000);
        var sale = MapToSale(command);

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _eventPublisher.Received(1).PublishAsync(
            Arg.Is<SaleCreatedEvent>(e => e.SaleId == sale.Id && e.Total == 8000),
            Arg.Any<CancellationToken>());
    }
}
