using System;
using System.Collections.Generic;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class SaleProductResultDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitPrice { get; set; }
    public int Quantity { get; set; }

    // Discount percentage (0, 10 or 20) applied based on quantity tiers
    public int Discount { get; set; }
    public int TotalAmount { get; set; }
}
