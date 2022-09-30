namespace Logistics.Application.Handlers.Commands;

public class RemoveEmployeeRoleHandler : RequestHandlerBase<RemoveEmployeeRoleCommand, DataResult>
{
    private readonly ITenantRepository _tenantRepository;

    public RemoveEmployeeRoleHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<DataResult> HandleValidated(
        RemoveEmployeeRoleCommand request, CancellationToken cancellationToken)
    {
        request.Role = request.Role?.ToLower();
        var employee = await _tenantRepository.GetAsync<Employee>(request.UserId);

        if (employee == null)
            return DataResult.CreateError("Could not find the specified user");

        var tenantRole = await _tenantRepository.GetAsync<TenantRole>(i => i.Name == request.Role);
        
        if (tenantRole == null)
            return DataResult.CreateError("Could not find the specified role name");

        employee.Roles.Remove(tenantRole);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(
        RemoveEmployeeRoleCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.UserId))
        {
            errorDescription = "UserId is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Role))
        {
            errorDescription = "Role name is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}