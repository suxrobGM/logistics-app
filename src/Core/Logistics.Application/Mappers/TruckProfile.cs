namespace Logistics.Application.Mappers;

internal class TruckProfile : Profile
{
    public TruckProfile()
    {
        CreateMap<TruckDto, CreateTruckCommand>().ReverseMap();
        CreateMap<TruckDto, UpdateTruckCommand>().ReverseMap();
    }
}
