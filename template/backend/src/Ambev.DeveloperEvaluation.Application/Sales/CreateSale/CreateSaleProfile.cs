using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        CreateMap<CreateSaleCommand, Sale>()
            .ForMember(d => d.SaleProducts, o => o.MapFrom(s => s.Products));

        CreateMap<SaleProductDto, SaleProduct>();

        CreateMap<Sale, CreateSaleResult>()
            .ForMember(d => d.Products, o => o.MapFrom(s => s.SaleProducts));
    }
}
