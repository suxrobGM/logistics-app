namespace Logistics.Application.Mappers;

internal class LoadProfile : Profile
{
    public LoadProfile()
    {
        CreateMap<LoadDto, CreateLoadCommand>();
        CreateMap<LoadDto, UpdateLoadCommand>();
    }
}
