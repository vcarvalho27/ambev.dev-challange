namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;

public class CancelSaleResponse
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
