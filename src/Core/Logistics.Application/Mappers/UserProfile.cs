namespace Logistics.Application.Mappers;

internal class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, CreateUserCommand>().ReverseMap();
        CreateMap<User, UpdateUserCommand>().ReverseMap();
        CreateMap<UserDto, CreateUserCommand>().ReverseMap();
        CreateMap<UserDto, UpdateUserCommand>().ReverseMap();
    }
}
