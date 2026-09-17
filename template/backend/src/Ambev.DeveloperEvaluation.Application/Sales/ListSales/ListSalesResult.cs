namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesResult
{
    public IEnumerable<ListSaleItemResult> Items { get; set; } = Array.Empty<ListSaleItemResult>();
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
