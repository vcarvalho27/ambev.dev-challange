using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetAsync(request.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        sale.CustomerId = request.CustomerId;
        sale.CustomerName = request.CustomerName;
        sale.BranchId = request.BranchId;
        sale.BranchName = request.BranchName;        

        sale.SaleProducts = request.Products.Select(p => _mapper.Map<SaleProduct>(p)).ToList();
        SalePricingCalculator.ApplyPricing(sale);

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleModifiedEvent
        {
            SaleId = updated.Id,
            SaleNumber = updated.SaleNumber,
            Total = updated.Total
        }, cancellationToken);

        return _mapper.Map<UpdateSaleResult>(updated);
    }
}
