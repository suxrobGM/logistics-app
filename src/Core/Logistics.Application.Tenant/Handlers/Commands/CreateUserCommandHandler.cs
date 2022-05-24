namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateUserCommandHandler : RequestHandlerBase<CreateUserCommand, DataResult>
{
    private readonly IMapper mapper;
    private readonly ITenantRepository<User> userRepository;

    public CreateUserCommandHandler(
        IMapper mapper,
        ITenantRepository<User> userRepository)
    {
        this.mapper = mapper;
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        await userRepository.AddAsync(mapper.Map<User>(request));
        await userRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateUserCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.ExternalId))
        {
            errorDescription = "External Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.FirstName))
        {
            errorDescription = "First name is an empty string";
        }
        else if (string.IsNullOrEmpty(request.LastName))
        {
            errorDescription = "Last name is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Email))
        {
            errorDescription = "Email is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
