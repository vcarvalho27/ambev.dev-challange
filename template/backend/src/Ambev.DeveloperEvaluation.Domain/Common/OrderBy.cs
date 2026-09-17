namespace Ambev.DeveloperEvaluation.Domain.Common;

public enum Ordering
{
    Asc,
    Desc
}

public class OrderBy
{
    public string Field { get; set; } = string.Empty;
    public Ordering Direction { get; set; } = Ordering.Asc;
}
