namespace Logistics.Application.Mappers;

internal class TenantProfile : Profile
{
    public TenantProfile()
    {
        CreateMap<Tenant, CreateTenantCommand>();
        CreateMap<Tenant, UpdateTenantCommand>();
        CreateMap<TenantDto, CreateTenantCommand>();
        CreateMap<TenantDto, UpdateTenantCommand>();
    }
}
