namespace Logistics.Application.Handlers.Queries;

internal sealed class UserExistsQueryHandler : RequestHandlerBase<UserExistsQuery, DataResult<UserDto>>
{
    private readonly IRepository<User> userRepository;

    public UserExistsQueryHandler(IRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult<UserDto>> HandleValidated(UserExistsQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await userRepository.GetAsync(i => i.ExternalId == request.ExternalId);

        if (userEntity == null)
        {
            return DataResult<UserDto>.CreateError("User not found");
        }

        var user = new UserDto
        {
            Id = userEntity.Id,
            ExternalId = userEntity.ExternalId,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            UserName = userEntity.UserName,
            Email = userEntity.Email,
            PhoneNumber = userEntity.PhoneNumber,
        };
        return DataResult<UserDto>.CreateSuccess(user);
    }

    protected override bool Validate(UserExistsQuery request, out string errorDescription)
    {
        if (string.IsNullOrEmpty(request.ExternalId))
        {
            errorDescription = "ExternalId is null or empty";
            return false;
        }

        errorDescription = string.Empty;
        return true;
    }
}
