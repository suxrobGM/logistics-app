namespace Logistics.Application.Mappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, CreateUserCommand>().ReverseMap();
        CreateMap<UserDto, CreateUserCommand>();
    }
}
