namespace Logistics.Application.Mappers;

internal class TenantProfile : Profile
{
    public TenantProfile()
    {
        CreateMap<TenantDto, CreateTenantCommand>();
        CreateMap<TenantDto, UpdateTenantCommand>();
    }
}
