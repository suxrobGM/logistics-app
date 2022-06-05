namespace Logistics.Application.Handlers.Queries;

internal sealed class GetUserRole : RequestHandlerBase<GetUserRoleQuery, DataResult<UserRoleDto>>
{
    private readonly ITenantRepository<User> _repository;

    public GetUserRole(ITenantRepository<User> repository)
    {
        _repository = repository;
    }

    protected override async Task<DataResult<UserRoleDto>> HandleValidated(GetUserRoleQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetAsync(i => i.Id == request.UserId || i.ExternalId == request.UserId);

        if (user == null)
        {
            return DataResult<UserRoleDto>.CreateError("Could not find the user");
        }

        var userRole = new UserRoleDto(user.Id, user.Role.Name);
        return DataResult<UserRoleDto>.CreateSuccess(userRole);
    }

    protected override bool Validate(GetUserRoleQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.UserId))
        {
            errorDescription = "User ID is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
