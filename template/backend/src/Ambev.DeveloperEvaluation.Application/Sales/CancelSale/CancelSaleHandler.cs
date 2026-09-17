using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleHandler(ISaleRepository saleRepository, IEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CancelSaleResult> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetAsync(request.Id, cancellationToken);
        if (sale == null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        sale.CancelledAt = DateTime.UtcNow;

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleCancelledEvent
        {
            SaleId = updated.Id,
            SaleNumber = updated.SaleNumber
        }, cancellationToken);

        return new CancelSaleResult { Id = updated.Id, Success = true, Message = "Sale cancelled" };
    }
}
