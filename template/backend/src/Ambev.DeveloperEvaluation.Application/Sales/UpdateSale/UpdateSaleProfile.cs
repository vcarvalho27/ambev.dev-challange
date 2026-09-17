using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleProfile : Profile
{
    public UpdateSaleProfile()
    {
        CreateMap<UpdateSaleCommand, Sale>()
            .ForMember(d => d.SaleProducts, o => o.MapFrom(s => s.Products));

        CreateMap<SaleProductDto, SaleProduct>();
        CreateMap<Sale, UpdateSaleResult>()
            .ForMember(d => d.Products, o => o.MapFrom(s => s.SaleProducts));
    }
}
