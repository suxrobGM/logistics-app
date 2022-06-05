namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateUserHandler : RequestHandlerBase<CreateUserCommand, DataResult>
{
    private readonly IMapper _mapper;
    private readonly ITenantRepository<User> _userRepository;

    public CreateUserHandler(
        IMapper mapper,
        ITenantRepository<User> userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        await _userRepository.AddAsync(_mapper.Map<User>(request));
        await _userRepository.UnitOfWork.CommitAsync();
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
