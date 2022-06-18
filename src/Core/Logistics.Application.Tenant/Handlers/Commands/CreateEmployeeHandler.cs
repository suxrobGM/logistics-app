namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateEmployeeHandler : RequestHandlerBase<CreateEmployeeCommand, DataResult>
{
    private readonly IMapper _mapper;
    private readonly ITenantRepository<Employee> _userRepository;

    public CreateEmployeeHandler(
        IMapper mapper,
        ITenantRepository<Employee> userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        await _userRepository.AddAsync(_mapper.Map<Employee>(request));
        await _userRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.ExternalId))
        {
            errorDescription = "External Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.UserName))
        {
            errorDescription = "UserName is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Email))
        {
            errorDescription = "Email is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
