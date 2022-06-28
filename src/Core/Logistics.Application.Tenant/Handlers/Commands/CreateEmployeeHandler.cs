namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateEmployeeHandler : RequestHandlerBase<CreateEmployeeCommand, DataResult>
{
    private readonly IMapper _mapper;
    private readonly ITenantRepository<Employee> _employeeRepository;
    private readonly IMainRepository<User> _userRepository;

    public CreateEmployeeHandler(
        IMapper mapper,
        ITenantRepository<Employee> employeeRepository,
        IMainRepository<User> userRepository)
    {
        _mapper = mapper;
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = _mapper.Map<Employee>(request);
        var user = await _userRepository.GetAsync(i => i.Id == employee.ExternalId);

        if (user == null)
        {
            return DataResult.CreateError("Could not find the specified user");
        }
        
        //user.JoinedTenants.Add();
        
        await _employeeRepository.AddAsync(employee);
        //await _userRepository.Update(user);
        //await _userRepository.UnitOfWork.CommitAsync();
        await _employeeRepository.UnitOfWork.CommitAsync();
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

        return string.IsNullOrEmpty(errorDescription);
    }
}
