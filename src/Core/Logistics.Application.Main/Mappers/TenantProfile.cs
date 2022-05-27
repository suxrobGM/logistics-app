namespace Logistics.Application.Mappers;

internal class TenantProfile : Profile
{
    public TenantProfile()
    {
        CreateMap<Tenant, CreateTenantCommand>().ReverseMap();
        CreateMap<Tenant, UpdateTenantCommand>().ReverseMap();
        CreateMap<TenantDto, CreateTenantCommand>().ReverseMap();
        CreateMap<TenantDto, UpdateTenantCommand>().ReverseMap();
    }
}
