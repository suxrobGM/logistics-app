namespace Logistics.Application.Handlers.Queries;

internal sealed class GetEmployeeByIdHandler : RequestHandlerBase<GetEmployeeByIdQuery, DataResult<EmployeeDto>>
{
    private readonly ITenantRepository<Employee> _userRepository;

    public GetEmployeeByIdHandler(
        ITenantRepository<Employee> userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task<DataResult<EmployeeDto>> HandleValidated(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await _userRepository.GetAsync(request.Id!) ??
                        await _userRepository.GetAsync(i => i.ExternalId == request.Id);

        if (userEntity == null)
        {
            return DataResult<EmployeeDto>.CreateError("Could not find the specified user");
        }

        var user = new EmployeeDto
        {
            Id = userEntity.Id,
            ExternalId = userEntity.ExternalId!,
            UserName = userEntity.UserName!,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Role = userEntity.Role.Name
        };

        return DataResult<EmployeeDto>.CreateSuccess(user);
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
