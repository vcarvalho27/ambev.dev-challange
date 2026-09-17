namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemResult
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
