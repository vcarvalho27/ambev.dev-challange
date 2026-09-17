namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

public class CancelSaleItemResponse
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
