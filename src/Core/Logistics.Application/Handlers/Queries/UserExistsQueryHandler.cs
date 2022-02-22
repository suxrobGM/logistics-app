namespace Logistics.Application.Handlers.Queries;

internal sealed class UserExistsQueryHandler : IRequestHandler<UserExistsQuery, DataResult<bool>>
{
    private readonly IRepository<User> userRepository;

    public UserExistsQueryHandler(IRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<DataResult<bool>> Handle(UserExistsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ExternalId))
        {
            return new() { Error = "ExternalId is null or empty" };
        }

        var user = await userRepository.GetAsync(i => i.ExternalId == request.ExternalId);

        if (user != null)
        {
            return DataResult<bool>.CreateSuccess(true);
        }

        return DataResult<bool>.CreateSuccess(false);
    }
}
