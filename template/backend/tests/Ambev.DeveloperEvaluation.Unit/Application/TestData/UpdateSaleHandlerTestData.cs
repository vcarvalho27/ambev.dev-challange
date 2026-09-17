using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

/// <summary>
/// Provides methods for generating test data using the Bogus library.
/// This class centralizes all test data generation to ensure consistency
/// across test cases and provide both valid and invalid data scenarios.
/// </summary>
public static class UpdateSaleHandlerTestData
{
    private static readonly Faker<SaleProductDto> SaleProductDtoFaker = new Faker<SaleProductDto>()
        .RuleFor(p => p.ProductId, f => f.Random.Guid())
        .RuleFor(p => p.ProductName, f => f.Commerce.ProductName())
        .RuleFor(p => p.UnitPrice, f => f.Random.Int(100, 10000))
        .RuleFor(p => p.Quantity, f => f.Random.Int(1, 3));

    private static readonly Faker<UpdateSaleCommand> updateSaleHandlerFaker = new Faker<UpdateSaleCommand>()
        .RuleFor(s => s.Id, f => f.Random.Guid())
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Products, f => SaleProductDtoFaker.Generate(f.Random.Int(1, 3)));

    /// <summary>
    /// Generates a valid UpdateSaleCommand with randomized data.
    /// </summary>
    public static UpdateSaleCommand GenerateValidCommand()
    {
        return updateSaleHandlerFaker.Generate();
    }

    /// <summary>
    /// Generates a valid UpdateSaleCommand with two product lines that share the same ProductId
    /// but have independent quantities/unit prices, so duplicate-product handling can be asserted.
    /// </summary>
    public static UpdateSaleCommand GenerateCommandWithDuplicateProductId(
        int firstQuantity, int firstUnitPrice, int secondQuantity, int secondUnitPrice)
    {
        var command = updateSaleHandlerFaker.Generate();
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
