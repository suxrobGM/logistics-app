using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Services;

public class UserService(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow) : IUserService
{
    public async Task UpdateUserAsync(UpdateUserParams userParams)
    {
        var userRepository = masterUow.Repository<User>();
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

        await masterUow.SaveChangesAsync();
    }

    private async Task UpdateTenantEmployeeDataAsync(Guid tenantId, User user)
    {
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var employeeRepository = tenantUow.Repository<Employee>();
        var employee = await employeeRepository.GetByIdAsync(user.Id);

        if (employee is null)
        {
            return;
        }

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email!;
        employee.PhoneNumber = user.PhoneNumber;
        await tenantUow.SaveChangesAsync();
    }
}
