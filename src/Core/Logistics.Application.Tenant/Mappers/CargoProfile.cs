namespace Logistics.Application.Mappers;

internal class CargoProfile : Profile
{
    public CargoProfile()
    {
        CreateMap<CargoDto, CreateCargoCommand>();
        CreateMap<CargoDto, UpdateCargoCommand>();
    }
}
