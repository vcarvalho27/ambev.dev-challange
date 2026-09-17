using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Unit.Domain.Services.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

/// <summary>
/// Contains unit tests for the <see cref="SalePricingCalculator"/> class.
/// Tests cover the quantity discount tiers and the aggregation of product
/// lines into the sale's Subtotal, Discount and Total.
/// </summary>
public class SalePricingCalculatorTests
{
    /// <summary>
    /// Tests that no discount is granted below the minimum discount tier.
    /// </summary>
    [Theory(DisplayName = "Quantity below 4 items should not have a discount")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Given_QuantityBelowFour_When_CalculatingDiscount_Then_ShouldBeZero(int quantity)
    {
        // Arrange & Act
        var discount = SalePricingCalculator.CalculateDiscountPercentage(quantity);

        // Assert
        discount.Should().Be(0);
    }

    /// <summary>
    /// Tests that a 10% discount is granted for quantities between 4 and 9 items.
    /// </summary>
    [Theory(DisplayName = "Quantity between 4 and 9 items should have a 10% discount")]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public void Given_QuantityBetweenFourAndNine_When_CalculatingDiscount_Then_ShouldBeTenPercent(int quantity)
    {
        // Arrange & Act
        var discount = SalePricingCalculator.CalculateDiscountPercentage(quantity);

        // Assert
        discount.Should().Be(10);
    }

    /// <summary>
    /// Tests that a 20% discount is granted for quantities between 10 and 20 items.
    /// </summary>
    [Theory(DisplayName = "Quantity between 10 and 20 items should have a 20% discount")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Given_QuantityBetweenTenAndTwenty_When_CalculatingDiscount_Then_ShouldBeTwentyPercent(int quantity)
    {
        // Arrange & Act
        var discount = SalePricingCalculator.CalculateDiscountPercentage(quantity);

        // Assert
        discount.Should().Be(20);
    }

    /// <summary>
    /// Tests that the product's discount and total amount are computed from its quantity and unit price.
    /// </summary>
    [Theory(DisplayName = "Product pricing should reflect the quantity discount tier")]
    [InlineData(1, 1000, 0, 1000)]
    [InlineData(3, 1000, 0, 3000)]
    [InlineData(4, 1000, 10, 3600)]
    [InlineData(9, 1000, 10, 8100)]
    [InlineData(10, 1000, 20, 8000)]
    [InlineData(20, 1000, 20, 16000)]
    public void Given_ProductQuantityAndUnitPrice_When_ApplyingPricing_Then_ShouldComputeDiscountAndTotalAmount(
        int quantity, int unitPrice, int expectedDiscount, int expectedTotalAmount)
    {
        // Arrange
        var product = SalePricingCalculatorTestData.GenerateProduct(quantity, unitPrice);

        // Act
        SalePricingCalculator.ApplyPricing(product);

        // Assert
        product.Discount.Should().Be(expectedDiscount);
        product.TotalAmount.Should().Be(expectedTotalAmount);
    }

    /// <summary>
    /// Tests that a sale with a single product rolls the product's pricing up into the sale totals.
    /// </summary>
    [Fact(DisplayName = "Sale with a single product should roll up Subtotal, Discount and Total")]
    public void Given_SaleWithSingleProduct_When_ApplyingPricing_Then_ShouldRollUpTotals()
    {
        // Arrange
        var product = SalePricingCalculatorTestData.GenerateProduct(quantity: 10, unitPrice: 1000);
        var sale = SalePricingCalculatorTestData.GenerateSale(product);

        // Act
        SalePricingCalculator.ApplyPricing(sale);

        // Assert
        sale.Subtotal.Should().Be(10000);
        sale.Total.Should().Be(8000);
        sale.Discount.Should().Be(2000);
    }

    /// <summary>
    /// Tests that a sale with multiple products, each on a different discount tier,
    /// has its totals computed as the sum of each product's own pricing.
    /// </summary>
    [Fact(DisplayName = "Sale with multiple products should sum each product's own pricing")]
    public void Given_SaleWithMultipleProducts_When_ApplyingPricing_Then_ShouldSumEachProductPricing()
    {
        // Arrange
        var belowTier = SalePricingCalculatorTestData.GenerateProduct(quantity: 2, unitPrice: 1000); // 0% -> 2000
        var mediumTier = SalePricingCalculatorTestData.GenerateProduct(quantity: 5, unitPrice: 1000); // 10% -> 4500
        var highTier = SalePricingCalculatorTestData.GenerateProduct(quantity: 10, unitPrice: 1000); // 20% -> 8000
        var sale = SalePricingCalculatorTestData.GenerateSale(belowTier, mediumTier, highTier);

        // Act
        SalePricingCalculator.ApplyPricing(sale);

        // Assert
        belowTier.Discount.Should().Be(0);
        mediumTier.Discount.Should().Be(10);
        highTier.Discount.Should().Be(20);

        sale.Subtotal.Should().Be(17000);
        sale.Total.Should().Be(14500);
        sale.Discount.Should().Be(2500);
    }

    /// <summary>
    /// Tests that a quantity above the highest configured tier's range still receives that
    /// tier's discount, since its upper bound is unbounded (MaxQuantity: null).
    /// </summary>
    [Theory(DisplayName = "Quantity above the highest tier should still receive the highest tier's discount")]
    [InlineData(21)]
    [InlineData(1000)]
    public void Given_QuantityAboveHighestTier_When_CalculatingDiscount_Then_ShouldUseHighestTierPercentage(int quantity)
    {
        // Arrange & Act
        var discount = SalePricingCalculator.CalculateDiscountPercentage(quantity);

        // Assert
        discount.Should().Be(20);
    }

    /// <summary>
    /// Tests <see cref="DiscountTier.Includes"/> boundary behavior directly, including the
    /// unbounded (MaxQuantity: null) case used by the highest tier.
    /// </summary>
    [Theory(DisplayName = "DiscountTier.Includes should respect its min and max boundaries")]
    [InlineData(4, 9, 3, false)]
    [InlineData(4, 9, 4, true)]
    [InlineData(4, 9, 9, true)]
    [InlineData(4, 9, 10, false)]
    [InlineData(10, null, 10, true)]
    [InlineData(10, null, 9, false)]
    [InlineData(10, null, int.MaxValue, true)]
    public void Given_QuantityAndTierBoundaries_When_CheckingIncludes_Then_ShouldRespectRange(
        int minQuantity, int? maxQuantity, int quantity, bool expected)
    {
        // Arrange
        var tier = new DiscountTier(minQuantity, maxQuantity, DiscountPercentage: 0);

        // Act
        var includes = tier.Includes(quantity);

        // Assert
        includes.Should().Be(expected);
    }

    /// <summary>
    /// Guards the tier table's own consistency: tiers must be ordered by ascending MinQuantity
    /// and must not overlap, so that adding a new tier can never silently shadow another one.
    /// </summary>
    [Fact(DisplayName = "Configured discount tiers should be ordered and non-overlapping")]
    public void Given_ConfiguredDiscountTiers_When_Inspected_Then_ShouldBeOrderedAndNonOverlapping()
    {
        // Arrange
        var tiers = SalePricingCalculator.DiscountTiers;

        // Assert
        for (var i = 1; i < tiers.Count; i++)
        {
            var previous = tiers[i - 1];
            var current = tiers[i];

            previous.MaxQuantity.Should().NotBeNull("only the last tier may be unbounded");
            current.MinQuantity.Should().BeGreaterThan(previous.MaxQuantity!.Value,
                "tiers must not overlap");
        }
    }
}
