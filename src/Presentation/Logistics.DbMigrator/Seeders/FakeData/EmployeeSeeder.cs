using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Seeders.Infrastructure;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds employees from created users, assigning roles.
/// </summary>
internal class EmployeeSeeder(ILogger<EmployeeSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(EmployeeSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 110;
    public override IReadOnlyList<string> DependsOn => [nameof(UserSeeder), nameof(DefaultTenantSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Employee>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var users = context.CreatedUsers ?? throw new InvalidOperationException("Users not seeded");
        var tenant = context.DefaultTenant ?? throw new InvalidOperationException("Default tenant not seeded");

        if (users.Count == 0)
        {
            Logger.LogWarning("No users available to create employees from");
            LogCompleted(0);
            return;
        }

        var roles = await context.TenantUnitOfWork.Repository<TenantRole>().GetListAsync(ct: cancellationToken);
        var ownerRole = roles.First(i => i.Name == TenantRoles.Owner);
        var managerRole = roles.First(i => i.Name == TenantRoles.Manager);
        var dispatcherRole = roles.First(i => i.Name == TenantRoles.Dispatcher);
        var driverRole = roles.First(i => i.Name == TenantRoles.Driver);

        var owner = users[0];
        var manager = users.Count > 1 ? users[1] : null;
        var dispatchers = users.Skip(2).Take(3);
        var drivers = users.Skip(5);

        var ownerEmployee = await CreateEmployeeAsync(context, tenant.Id, owner, 0, SalaryType.None, ownerRole);
        var managerEmployee = manager is not null
            ? await CreateEmployeeAsync(context, tenant.Id, manager, 5000, SalaryType.Monthly, managerRole)
            : ownerEmployee;

        var companyEmployees = new CompanyEmployees(ownerEmployee, managerEmployee);

        foreach (var dispatcher in dispatchers)
        {
            var dispatcherEmployee = await CreateEmployeeAsync(
                context, tenant.Id, dispatcher, 1000, SalaryType.Weekly, dispatcherRole);
            companyEmployees.Dispatchers.Add(dispatcherEmployee);
            companyEmployees.AllEmployees.Add(dispatcherEmployee);
        }

        foreach (var driver in drivers)
        {
            var driverEmployee = await CreateEmployeeAsync(
                context, tenant.Id, driver, 0.3M, SalaryType.ShareOfGross, driverRole);
            companyEmployees.Drivers.Add(driverEmployee);
            companyEmployees.AllEmployees.Add(driverEmployee);
        }

        companyEmployees.AllEmployees.Add(ownerEmployee);
        if (manager is not null)
        {
            companyEmployees.AllEmployees.Add(managerEmployee);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);

        context.CreatedEmployees = companyEmployees;
        LogCompleted(companyEmployees.AllEmployees.Count);
    }

    private async Task<Employee> CreateEmployeeAsync(
        SeederContext context,
        Guid tenantId,
        User user,
        decimal salary,
        SalaryType salaryType,
        TenantRole role)
    {
        var employeeRepository = context.TenantUnitOfWork.Repository<Employee>();
        var employee = await employeeRepository.GetByIdAsync(user.Id);

        if (employee is not null)
        {
            return employee;
        }

        employee = Employee.CreateEmployeeFromUser(user, salary, salaryType);
        user.TenantId = tenantId;
        await employeeRepository.AddAsync(employee);
        employee.Roles.Add(role);
        Logger.LogInformation("Created employee {Name} with role {Role}", user.UserName, role.Name);
        return employee;
    }
}
