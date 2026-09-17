using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Subtotal { get; set; }
    public int Discount { get; set; }
    public int Total { get; set; }
    public DateTime? CancelledAt { get; set; }
    public IEnumerable<SaleProductResultDto> Products { get; set; } = Array.Empty<SaleProductResultDto>();
}
