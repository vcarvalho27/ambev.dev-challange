using MediatR;
using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public record ListSalesQuery(int Page = 1, int Size = 10, IEnumerable<OrderBy>? Order = null) : IRequest<ListSalesResult>;
