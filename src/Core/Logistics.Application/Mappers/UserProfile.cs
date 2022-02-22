using AutoMapper;
using Logistics.Application.Contracts.Commands;
using Logistics.Domain.Entities;

namespace Logistics.Application.Mappers;

internal sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, CreateUserCommand>();
    }
}
