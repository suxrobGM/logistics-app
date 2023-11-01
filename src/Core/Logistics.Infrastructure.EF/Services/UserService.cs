using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;

namespace Logistics.Infrastructure.EF.Services;

public class UserService : IUserService
{
    private readonly IMasterRepository _masterRepository;
    private readonly ITenantRepository _tenantRepository;

    public UserService(
        IMasterRepository masterRepository,
        ITenantRepository tenantRepository)
    {
        _masterRepository = masterRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task UpdateUserAsync(UpdateUserData userData)
    {
        var user = await _masterRepository.GetAsync<User>(userData.Id);

        if (user == null)
            return;

        if (!string.IsNullOrEmpty(userData.FirstName))
            user.FirstName = userData.FirstName;

        if (!string.IsNullOrEmpty(userData.LastName))
            user.LastName = userData.LastName;

        if (!string.IsNullOrEmpty(userData.PhoneNumber))
            user.PhoneNumber = userData.PhoneNumber;

        var tenantIds = user.GetJoinedTenantIds();

        foreach (var tenantId in tenantIds)
        {
            await UpdateTenantEmployeeDataAsync(tenantId, user);
        }
        
        _masterRepository.Update(user);
        await _masterRepository.UnitOfWork.CommitAsync();
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
