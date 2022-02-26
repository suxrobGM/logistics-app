namespace Logistics.Application.Handlers.Queries;

internal sealed class UserExistsQueryHandler : RequestHandlerBase<UserExistsQuery, DataResult<bool>>
{
    private readonly IRepository<User> userRepository;

    public UserExistsQueryHandler(IRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult<bool>> HandleValidated(UserExistsQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(i => i.ExternalId == request.ExternalId);

        if (user != null)
        {
            return DataResult<bool>.CreateSuccess(true);
        }

        return DataResult<bool>.CreateSuccess(false);
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
