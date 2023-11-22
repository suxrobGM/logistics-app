using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;

namespace Logistics.Infrastructure.EF.Services;

public class UserService : IUserService
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly ITenantUnityOfWork _tenantUow;

    public UserService(
        IMasterUnityOfWork masterUow,
        ITenantUnityOfWork tenantUow)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
    }

    public async Task UpdateUserAsync(UpdateUserData userData)
    {
        var userRepository = _masterUow.Repository<User>();
        var user = await userRepository.GetByIdAsync(userData.Id);

        if (user is null)
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
        
        userRepository.Update(user);
        await _masterUow.SaveChangesAsync();
    }

    private async Task UpdateTenantEmployeeDataAsync(string tenantId, User user)
    {
        _tenantUow.SetCurrentTenantById(tenantId);
        var employeeRepository = _tenantUow.Repository<Employee>();
        var employee = await employeeRepository.GetByIdAsync(user.Id);

        if (employee is null)
            return;

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email;
        employee.PhoneNumber = user.PhoneNumber;
        employeeRepository.Update(employee);
        await _tenantUow.SaveChangesAsync();
    }
}
