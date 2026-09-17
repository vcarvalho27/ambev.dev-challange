using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Common.Exceptions;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);


        var existsSales = await _saleRepository.ExistsSaleNumberAsync(command.SaleNumber, cancellationToken);
        if (existsSales)
            throw new UnprocessableEntityException($"Sale with number {command.SaleNumber} already exists");

        var sale = _mapper.Map<Sale>(command);
        SalePricingCalculator.ApplyPricing(sale);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleCreatedEvent
        {
            SaleId = created.Id,
            SaleNumber = created.SaleNumber,
            CustomerId = created.CustomerId,
            BranchId = created.BranchId,
            Total = created.Total
        }, cancellationToken);

        return _mapper.Map<CreateSaleResult>(created);
    }
}
