using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// This class centralizes all test data generation to ensure consistency
/// across test cases and provide both valid and invalid data scenarios.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker<SaleProductDto> SaleProductDtoFaker = new Faker<SaleProductDto>()
        .RuleFor(p => p.ProductId, f => f.Random.Guid())
        .RuleFor(p => p.ProductName, f => f.Commerce.ProductName())
        .RuleFor(p => p.UnitPrice, f => f.Random.Int(100, 10000))
        .RuleFor(p => p.Quantity, f => f.Random.Int(1, 3));

    /// <summary>
    /// Configures the Faker to generate valid CreateSaleCommand requests.
    /// The generated commands will have valid:
    /// - CustomerId/CustomerName and BranchId/BranchName
    /// - One to three product lines, each with quantities below the discount threshold
    /// </summary>
    private static readonly Faker<CreateSaleCommand> createSaleHandlerFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Products, f => SaleProductDtoFaker.Generate(f.Random.Int(1, 3)));

    /// <summary>
    /// Generates a valid CreateSaleCommand with randomized data.
    /// </summary>
    /// <returns>A valid CreateSaleCommand with randomly generated data.</returns>
    public static CreateSaleCommand GenerateValidCommand()
    {
        return createSaleHandlerFaker.Generate();
    }

    /// <summary>
    /// Generates a valid CreateSaleCommand with a single product line, allowing the
    /// quantity and unit price to be controlled so discount tier behavior can be asserted.
    /// </summary>
    /// <param name="quantity">The quantity for the single product line.</param>
    /// <param name="unitPrice">The unit price for the single product line.</param>
    /// <returns>A valid CreateSaleCommand containing a single product line.</returns>
    public static CreateSaleCommand GenerateCommandWithSingleProduct(int quantity, int unitPrice = 1000)
    {
        var command = createSaleHandlerFaker.Generate();
        var product = SaleProductDtoFaker.Generate();
        product.Quantity = quantity;
        product.UnitPrice = unitPrice;
        command.Products = new[] { product };
        return command;
    }

    /// <summary>
    /// Generates a valid CreateSaleCommand with two product lines that share the same ProductId
    /// but have independent quantities/unit prices, so duplicate-product handling can be asserted.
    /// </summary>
    public static CreateSaleCommand GenerateCommandWithDuplicateProductId(
        int firstQuantity, int firstUnitPrice, int secondQuantity, int secondUnitPrice)
    {
        var command = createSaleHandlerFaker.Generate();
        var sharedProductId = Guid.NewGuid();

        var first = SaleProductDtoFaker.Generate();
        first.ProductId = sharedProductId;
        first.Quantity = firstQuantity;
        first.UnitPrice = firstUnitPrice;

        var second = SaleProductDtoFaker.Generate();
        second.ProductId = sharedProductId;
        second.Quantity = secondQuantity;
        second.UnitPrice = secondUnitPrice;

        command.Products = new[] { first, second };
        return command;
    }
}
