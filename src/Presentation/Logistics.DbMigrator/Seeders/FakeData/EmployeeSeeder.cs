using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
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
    public override IReadOnlyList<string> DependsOn => [nameof(UserSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Employee>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var tenant = context.CurrentTenant ?? throw new InvalidOperationException("Current tenant not set");
        var seedUsers = LoadSeedUsers(context);

        // Load users from shared context or from database in config order (if UserSeeder was skipped)
        var users = context.CreatedUsers;
        if (users is null || users.Count == 0)
        {
            users = await LoadUsersInConfigOrderAsync(context, seedUsers, cancellationToken);
        }

        if (users.Count == 0)
        {
            logger.LogWarning("No users available to create employees from");
            LogCompleted(0);
            return;
        }

        var roles = await context.TenantUnitOfWork.Repository<TenantRole>().GetListAsync(ct: cancellationToken);
        var ownerRole = roles.First(i => i.Name == TenantRoles.Owner);
        var managerRole = roles.First(i => i.Name == TenantRoles.Manager);
        var dispatcherRole = roles.First(i => i.Name == TenantRoles.Dispatcher);
        var driverRole = roles.First(i => i.Name == TenantRoles.Driver);

        var split = SplitUsersByRole(users, seedUsers);

        var ownerEmployee = await CreateEmployeeAsync(context, tenant.Id, split.Owner, 0, SalaryType.None, ownerRole);
        var managerEmployee = split.Manager is not null
            ? await CreateEmployeeAsync(context, tenant.Id, split.Manager, 5000, SalaryType.Monthly, managerRole)
            : ownerEmployee;

        var companyEmployees = new CompanyEmployees(ownerEmployee, managerEmployee);

        foreach (var dispatcher in split.Dispatchers)
        {
            var dispatcherEmployee = await CreateEmployeeAsync(
                context, tenant.Id, dispatcher, 1000, SalaryType.Weekly, dispatcherRole);
            companyEmployees.Dispatchers.Add(dispatcherEmployee);
            companyEmployees.AllEmployees.Add(dispatcherEmployee);
        }

        foreach (var driver in split.Drivers)
        {
            var driverEmployee = await CreateEmployeeAsync(
                context, tenant.Id, driver, 0.3M, SalaryType.ShareOfGross, driverRole);
            companyEmployees.Drivers.Add(driverEmployee);
            companyEmployees.AllEmployees.Add(driverEmployee);
        }

        companyEmployees.AllEmployees.Add(ownerEmployee);
        if (split.Manager is not null)
        {
            companyEmployees.AllEmployees.Add(managerEmployee);
        }

        // An owner-operator drives their own truck. Registering them as a driver here (after
        // AllEmployees is built, so they are not counted twice) is what gives TruckSeeder and the
        // driver-scoped safety seeders something to work with.
        if (companyEmployees.Drivers.Count == 0)
        {
            companyEmployees.Drivers.Add(ownerEmployee);
            logger.LogInformation("No dedicated drivers seeded - {Owner} drives their own truck",
                split.Owner.UserName);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);

        context.CreatedEmployees = companyEmployees;
        LogCompleted(companyEmployees.AllEmployees.Count);
    }

    private static UserData[] LoadSeedUsers(SeederContext context)
    {
        var seedKey = context.SeedDataKey;
        if (string.IsNullOrEmpty(seedKey)) return [];

        return context.Configuration.GetSection($"{seedKey}:Users").Get<UserData[]>() ?? [];
    }

    /// <summary>
    /// Loads users from database in the same order as the seed data, to preserve role assignments.
    /// </summary>
    private static async Task<IList<User>> LoadUsersInConfigOrderAsync(
        SeederContext context, UserData[] seedUsers, CancellationToken ct)
    {
        var orderedUsers = new List<User>();
        foreach (var fakeUser in seedUsers)
        {
            var user = await context.UserManager.FindByNameAsync(fakeUser.Email);
            if (user is not null)
                orderedUsers.Add(user);
        }

        return orderedUsers;
    }

    /// <summary>
    /// Maps users onto company roles. When the seed data declares an explicit <c>Role</c> the
    /// mapping is by role name; otherwise it falls back to the positional convention the fleet
    /// seed sets rely on (owner, manager, three dispatchers, the rest drivers).
    /// </summary>
    private UserRoleSplit SplitUsersByRole(IList<User> users, UserData[] seedUsers)
    {
        var rolesByEmail = seedUsers
            .Where(u => !string.IsNullOrWhiteSpace(u.Role))
            .ToDictionary(u => u.Email, u => u.Role!, StringComparer.OrdinalIgnoreCase);

        if (rolesByEmail.Count == 0)
        {
            return new UserRoleSplit(
                users[0],
                users.Count > 1 ? users[1] : null,
                [.. users.Skip(2).Take(3)],
                [.. users.Skip(5)]);
        }

        User? owner = null;
        User? manager = null;
        var dispatchers = new List<User>();
        var drivers = new List<User>();

        foreach (var user in users)
        {
            var key = user.Email ?? user.UserName ?? string.Empty;
            var role = rolesByEmail.GetValueOrDefault(key, TenantRoles.Driver);

            switch (role)
            {
                case TenantRoles.Owner:
                    owner ??= user;
                    break;
                case TenantRoles.Manager:
                    manager ??= user;
                    break;
                case TenantRoles.Dispatcher:
                    dispatchers.Add(user);
                    break;
                case TenantRoles.Driver:
                    drivers.Add(user);
                    break;
                default:
                    logger.LogWarning("Unknown seed role '{Role}' for {Email} - treating as driver",
                        role, key);
                    drivers.Add(user);
                    break;
            }
        }

        return new UserRoleSplit(owner ?? users[0], manager, dispatchers, drivers);
    }

    private sealed record UserRoleSplit(
        User Owner,
        User? Manager,
        List<User> Dispatchers,
        List<User> Drivers);

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
        employee.Address = context.Region?.CompanyAddress;
        user.TenantId = tenantId;
        await employeeRepository.AddAsync(employee);
        employee.Role = role;
        logger.LogInformation("Created employee {Name} with role {Role}", user.UserName, role.Name);
        return employee;
    }
}
