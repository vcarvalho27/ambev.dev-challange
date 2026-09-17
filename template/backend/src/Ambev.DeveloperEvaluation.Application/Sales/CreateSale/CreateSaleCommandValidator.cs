using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
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
                .LessThanOrEqualTo(Domain.Services.SalePricingCalculator.MaxQuantityPerProduct)
                .WithMessage($"Quantity cannot exceed {Domain.Services.SalePricingCalculator.MaxQuantityPerProduct} items per product");
            p.RuleFor(q => q.UnitPrice).GreaterThan(0);
        });
    }

    private static bool HaveNoDuplicateProductIds(IEnumerable<Common.SaleProductDto> products)
    {
        var productIds = products.Select(p => p.ProductId).ToList();
        return productIds.Count == productIds.Distinct().Count();
    }
}
