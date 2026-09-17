using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services.TestData;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// This class centralizes all test data generation to ensure consistency
/// across test cases and provide both valid and invalid discount-tier scenarios.
/// </summary>
public static class SalePricingCalculatorTestData
{
    private static readonly Faker<SaleProduct> SaleProductFaker = new Faker<SaleProduct>()
        .RuleFor(p => p.ProductId, f => f.Random.Guid())
        .RuleFor(p => p.ProductName, f => f.Commerce.ProductName())
        .RuleFor(p => p.UnitPrice, f => f.Random.Int(100, 10000))
        .RuleFor(p => p.Quantity, f => 1);

    /// <summary>
    /// Generates a single valid SaleProduct with the given quantity and unit price.
    /// </summary>
    public static SaleProduct GenerateProduct(int quantity, int unitPrice = 1000)
    {
        var product = SaleProductFaker.Generate();
        product.Quantity = quantity;
        product.UnitPrice = unitPrice;
        return product;
    }

    /// <summary>
    /// Generates a valid Sale containing the given product lines.
    /// </summary>
    public static Sale GenerateSale(params SaleProduct[] products)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = new Faker().Commerce.Ean13(),
            CustomerId = Guid.NewGuid(),
            CustomerName = new Faker().Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = new Faker().Company.CompanyName(),
            SaleProducts = products.ToList()
        };
    }
}
