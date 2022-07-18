namespace Logistics.Application.Handlers.Queries;

internal sealed class GetEmployeeByIdHandler : RequestHandlerBase<GetEmployeeByIdQuery, DataResult<EmployeeDto>>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Employee> _employeeRepository;

    public GetEmployeeByIdHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Employee> employeeRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
    }

    protected override async Task<DataResult<EmployeeDto>> HandleValidated(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employeeEntity = await _employeeRepository.GetAsync(i => i.Id == request.Id || i.ExternalId == request.Id);
        
        if (employeeEntity == null)
            return DataResult<EmployeeDto>.CreateError("Could not find the specified employee");

        var userEntity = await _userRepository.GetAsync(i => i.Id == employeeEntity.ExternalId);

        var employee = new EmployeeDto
        {
            Id = employeeEntity.Id,
            ExternalId = employeeEntity.ExternalId!,
            UserName = employeeEntity.UserName!,
            FirstName = employeeEntity.FirstName,
            LastName = employeeEntity.LastName,
            Email = userEntity?.Email,
            PhoneNumber = userEntity?.PhoneNumber,
            Role = employeeEntity.Role.Name,
            JoinedDate = employeeEntity.JoinedDate
        };

        return DataResult<EmployeeDto>.CreateSuccess(employee);
    }

    protected override bool Validate(GetEmployeeByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
