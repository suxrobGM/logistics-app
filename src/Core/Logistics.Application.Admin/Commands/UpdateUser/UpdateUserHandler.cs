using Logistics.Application.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateUserHandler : RequestHandler<UpdateUserCommand, ResponseResult>
{
    private readonly IMasterRepository _masterRepository;
    private readonly ITenantRepository _tenantRepository;

    public UpdateUserHandler(
        IMasterRepository masterRepository,
        ITenantRepository tenantRepository)
    {
        _masterRepository = masterRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateUserCommand req, CancellationToken cancellationToken)
    {
        var user = await _masterRepository.GetAsync<User>(req.Id);

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
        
        _masterRepository.Update(user);
        await _masterRepository.UnitOfWork.CommitAsync();
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
