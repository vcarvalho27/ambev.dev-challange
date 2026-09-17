using System;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Base payload shared by every sale-related integration event.
/// </summary>
public abstract class SaleEvent
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when a new sale is created.
/// </summary>
public class SaleCreatedEvent : SaleEvent
{
    public Guid CustomerId { get; set; }
    public Guid BranchId { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// Raised when an existing sale is modified.
/// </summary>
public class SaleModifiedEvent : SaleEvent
{
    public int Total { get; set; }
}

/// <summary>
/// Raised when a sale is cancelled.
/// </summary>
public class SaleCancelledEvent : SaleEvent
{
}

/// <summary>
/// Raised when a single item within a sale is cancelled.
/// </summary>
public class ItemCancelledEvent : SaleEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
}
