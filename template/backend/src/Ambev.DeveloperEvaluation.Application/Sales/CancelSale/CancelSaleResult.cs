namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleResult
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
