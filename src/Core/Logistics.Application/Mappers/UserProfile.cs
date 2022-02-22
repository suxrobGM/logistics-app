namespace Logistics.Application.Mappers;

internal sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, CreateUserCommand>();
        CreateMap<UserDto, CreateUserCommand>();
    }
}
