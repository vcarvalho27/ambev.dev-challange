using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Services;

/// <summary>
/// A quantity discount tier: a product line whose quantity falls within
/// [<see cref="MinQuantity"/>, <see cref="MaxQuantity"/>] receives <see cref="DiscountPercentage"/>% off.
/// A null <see cref="MaxQuantity"/> means the tier has no upper bound.
/// </summary>
public readonly record struct DiscountTier(int MinQuantity, int? MaxQuantity, int DiscountPercentage)
{
    public bool Includes(int quantity) => quantity >= MinQuantity && (MaxQuantity is null || quantity <= MaxQuantity);
}

/// <summary>
/// Computes per-product discounts and sale totals from the quantity discount tiers.
/// A single product line is capped at <see cref="MaxQuantityPerProduct"/> items.
/// </summary>
public static class SalePricingCalculator
{
    public const int MaxQuantityPerProduct = 20;

    /// <summary>
    /// The quantity discount tiers. To add, change or remove a tier, edit this list only -
    /// no other code in <see cref="SalePricingCalculator"/> needs to change.
    /// Tier ranges must not overlap; the first tier whose range includes the quantity is applied,
    /// and a quantity that falls in no tier gets no discount.
    /// </summary>
    public static readonly IReadOnlyList<DiscountTier> DiscountTiers = new[]
    {
        new DiscountTier(MinQuantity: 4, MaxQuantity: 9, DiscountPercentage: 10),
        new DiscountTier(MinQuantity: 10, MaxQuantity: null, DiscountPercentage: 20),
    };

    /// <summary>
    /// Returns the discount percentage (0 when no tier applies) for the given quantity.
    /// </summary>
    public static int CalculateDiscountPercentage(int quantity)
    {
        foreach (var tier in DiscountTiers)
        {
            if (tier.Includes(quantity))
                return tier.DiscountPercentage;
        }

        return 0;
    }

    /// <summary>
    /// Recalculates a product line's discount percentage and total amount from its quantity and unit price.
    /// </summary>
    public static void ApplyPricing(SaleProduct product)
    {
        product.Discount = CalculateDiscountPercentage(product.Quantity);

        var grossAmount = product.UnitPrice * product.Quantity;
        var discountAmount = grossAmount * product.Discount / 100;

        product.TotalAmount = grossAmount - discountAmount;
    }

    /// <summary>
    /// Recalculates every product line of the sale and rolls the results up into the sale's Subtotal, Discount and Total.
    /// </summary>
    public static void ApplyPricing(Sale sale)
    {
        var subtotal = 0;
        var total = 0;

        foreach (var product in sale.SaleProducts)
        {
            ApplyPricing(product);
            subtotal += product.UnitPrice * product.Quantity;
            total += product.TotalAmount;
        }

        sale.Subtotal = subtotal;
        sale.Total = total;
        sale.Discount = subtotal - total;
    }
}
