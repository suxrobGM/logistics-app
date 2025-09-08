using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IMasterUnitOfWork _masterUow;
    private readonly ITenantUnitOfWork _tenantUow;

    public UserService(
        IMasterUnitOfWork masterUow,
        ITenantUnitOfWork tenantUow)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
    }

    public async Task UpdateUserAsync(UpdateUserParams userParams)
    {
        var userRepository = _masterUow.Repository<User>();
        var user = await userRepository.GetByIdAsync(userParams.Id);

        if (user is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(userParams.FirstName))
        {
            user.FirstName = userParams.FirstName;
        }

        if (!string.IsNullOrEmpty(userParams.LastName))
        {
            user.LastName = userParams.LastName;
        }

        if (!string.IsNullOrEmpty(userParams.PhoneNumber))
        {
            user.PhoneNumber = userParams.PhoneNumber;
        }

        if (userParams.TenantId.HasValue)
        {
            await UpdateTenantEmployeeDataAsync(userParams.TenantId.Value, user);
        }

        await _masterUow.SaveChangesAsync();
    }

    private async Task UpdateTenantEmployeeDataAsync(Guid tenantId, User user)
    {
        await _tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var employeeRepository = _tenantUow.Repository<Employee>();
        var employee = await employeeRepository.GetByIdAsync(user.Id);

        if (employee is null)
        {
            return;
        }

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email;
        employee.PhoneNumber = user.PhoneNumber;
        await _tenantUow.SaveChangesAsync();
    }
}
