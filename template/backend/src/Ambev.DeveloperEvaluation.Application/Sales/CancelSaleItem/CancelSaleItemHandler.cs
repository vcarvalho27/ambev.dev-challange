using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Services;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetWithProductsAsync(request.SaleId, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        var product = sale.SaleProducts.FirstOrDefault(p => p.ProductId == request.ProductId);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {request.ProductId} not found in sale {request.SaleId}");

        sale.SaleProducts.Remove(product);
        SalePricingCalculator.ApplyPricing(sale);

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new ItemCancelledEvent
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber,
            ProductId = product.ProductId,
            ProductName = product.ProductName
        }, cancellationToken);

        return new CancelSaleItemResult
        {
            SaleId = sale.Id,
            ProductId = product.ProductId,
            Success = true,
            Message = "Item cancelled"
        };
    }
}
