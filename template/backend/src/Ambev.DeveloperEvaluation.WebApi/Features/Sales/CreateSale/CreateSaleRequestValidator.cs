using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleRequest
/// </summary>
public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty();
        RuleFor(x => x.Products).NotEmpty();
        RuleFor(x => x.Products)
            .Must(HaveNoDuplicateProductIds)
            .WithMessage("Sale cannot contain more than one product line with the same product id");
        RuleForEach(x => x.Products).ChildRules(p =>
        {
            p.RuleFor(q => q.ProductId).NotEmpty();
            p.RuleFor(q => q.Quantity)
                .GreaterThan(0)
                .LessThanOrEqualTo(Ambev.DeveloperEvaluation.Domain.Services.SalePricingCalculator.MaxQuantityPerProduct)
                .WithMessage($"Quantity cannot exceed {Ambev.DeveloperEvaluation.Domain.Services.SalePricingCalculator.MaxQuantityPerProduct} items per product");
            p.RuleFor(q => q.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }

    private static bool HaveNoDuplicateProductIds(IEnumerable<Ambev.DeveloperEvaluation.Application.Sales.Common.SaleProductDto> products)
    {
        var productIds = products.Select(p => p.ProductId).ToList();
        return productIds.Count == productIds.Distinct().Count();
    }
}
