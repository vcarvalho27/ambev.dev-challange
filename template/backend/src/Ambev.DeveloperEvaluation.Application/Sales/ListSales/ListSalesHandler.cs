using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _saleRepository.ListWithProductsAsync(request.Page, request.Size, request.Order, cancellationToken);
        var mapped = _mapper.Map<IEnumerable<ListSaleItemResult>>(items);
        var totalPages = (int)Math.Ceiling(total / (double)request.Size);
        return new ListSalesResult
        {
            Items = mapped,
            Page = request.Page,
            Size = request.Size,
            TotalCount = total,
            TotalPages = totalPages
        };
    }
}
