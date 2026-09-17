using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty();
        RuleFor(x => x.Products).NotNull();
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
        });
    }

    private static bool HaveNoDuplicateProductIds(IEnumerable<Common.SaleProductDto> products)
    {
        var productIds = products.Select(p => p.ProductId).ToList();
        return productIds.Count == productIds.Distinct().Count();
    }
}
