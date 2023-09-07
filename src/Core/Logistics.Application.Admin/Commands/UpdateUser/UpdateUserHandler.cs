namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateUserHandler : RequestHandler<UpdateUserCommand, ResponseResult>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public UpdateUserHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateUserCommand req, CancellationToken cancellationToken)
    {
        var user = await _mainRepository.GetAsync<User>(req.Id);

        if (user == null)
            return ResponseResult.CreateError("Could not find the specified user");

        if (!string.IsNullOrEmpty(req.FirstName))
            user.FirstName = req.FirstName;

        if (!string.IsNullOrEmpty(req.LastName))
            user.LastName = req.LastName;

        if (!string.IsNullOrEmpty(req.PhoneNumber))
            user.PhoneNumber = req.PhoneNumber;

        var tenantIds = user.GetJoinedTenantIds();

        foreach (var tenantId in tenantIds)
        {
            await UpdateTenantEmployeeDataAsync(tenantId, user);
        }
        
        _mainRepository.Update(user);
        await _mainRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }

    private async Task UpdateTenantEmployeeDataAsync(string tenantId, User user)
    {
        _tenantRepository.SetTenantId(tenantId);
        var employee = await _tenantRepository.GetAsync<Employee>(user.Id);

        if (employee is null)
            return;

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email;
        employee.PhoneNumber = user.PhoneNumber;
        _tenantRepository.Update(employee);
        await _tenantRepository.UnitOfWork.CommitAsync();
    }
}
