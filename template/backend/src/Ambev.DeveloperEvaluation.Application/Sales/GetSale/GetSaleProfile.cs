using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Application.Sales.Common;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleProfile : Profile
{
    public GetSaleProfile()
    {
        CreateMap<SaleProduct, SaleProductResultDto>();
        CreateMap<Sale, GetSaleResult>()
            .ForMember(d => d.Products, o => o.MapFrom(s => s.SaleProducts));
    }
}
