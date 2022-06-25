namespace Logistics.Application.Handlers.Queries;

internal sealed class GetUserByIdHandler : RequestHandlerBase<GetUserByIdQuery, DataResult<UserDto>>
{
    private readonly IMainRepository<User> _userRepository;

    public GetUserByIdHandler(IMainRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task<DataResult<UserDto>> HandleValidated(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await _userRepository.GetAsync(request.Id!);

        if (userEntity == null)
        {
            return DataResult<UserDto>.CreateError("Could not find the specified user");
        }

        var user = new UserDto
        {
            Id = userEntity.Id,
            UserName = userEntity.UserName!,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Role = userEntity.Role.Name,
            Email = userEntity.Email,
            PhoneNumber = userEntity.PhoneNumber
        };

        return DataResult<UserDto>.CreateSuccess(user);
    }

    protected override bool Validate(GetUserByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
