using Logistics.Domain.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Services;

public class PopulateData
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public PopulateData(
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    public async Task ExecuteAsync()
    {
        try
        {
            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            var populateData = configuration.GetValue<bool>("PopulateData");

            if (!populateData)
                return;

            _logger.LogInformation("Populating databases with test data");
            var users = await AddUsersAsync(configuration);
            var employees = await AddEmployeesAsync(users);
            _logger.LogInformation("Databases have been populated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Thrown exception in PopulateData.ExecuteAsync(): {Exception}", ex);
        }
    }
    
    private async Task<IList<User>> AddUsersAsync(IConfiguration configuration)
    {
        var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        var testUsers = configuration.GetSection("Users").Get<UserDto[]>();
        var usersList = new List<User>();

        foreach (var testUser in testUsers)
        {
            var user = await userManager.FindByNameAsync(testUser.UserName);

            if (user != null)
            {
                usersList.Add(user);
                continue;
            }
            
            user = new User
            {
                UserName = testUser.UserName,
                Email = testUser.Email,
                EmailConfirmed = true
            };
            
            try
            {
                var result = await userManager.CreateAsync(user, testUser.Password);
                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);
                
                usersList.Add(user);
            }
            finally
            {
                _logger.LogInformation("Created an user {UserName}", testUser.UserName);
            }
        }
        
        return usersList;
    }

    private async Task<IList<Employee>> AddEmployeesAsync(IList<User> users)
    {
        if (users.Count < 10)
        {
            throw new InvalidOperationException("Add at least 10 test users in the 'testData.json' under the `Users` section");
        }
        
        var repository = _serviceProvider.GetRequiredService<ITenantRepository>();
        var owner = users[0];
        var manager = users[1];
        var dispatchers = users.Skip(2).Take(3);
        var drivers = users.Skip(5);
        var employeesList = new List<Employee>();
    
        var ownerRole = await repository.GetAsync<TenantRole>(i => i.Name == TenantRoles.Owner);
        var managerRole = await repository.GetAsync<TenantRole>(i => i.Name == TenantRoles.Manager);
        var dispatcherRole = await repository.GetAsync<TenantRole>(i => i.Name == TenantRoles.Dispatcher);
        var driverRole = await repository.GetAsync<TenantRole>(i => i.Name == TenantRoles.Driver);

        var ownerEmployee = await TryAddEmployeeAsync(repository, owner, ownerRole!);
        var managerEmployee = await TryAddEmployeeAsync(repository, manager, managerRole!);
        employeesList.Add(ownerEmployee);
        employeesList.Add(managerEmployee);
        
        foreach (var dispatcher in dispatchers)
        {
            var dispatcherEmployee = await TryAddEmployeeAsync(repository, dispatcher, dispatcherRole!);
            employeesList.Add(dispatcherEmployee);
        }
        
        foreach (var driver in drivers)
        {
            var driverEmployee = await TryAddEmployeeAsync(repository, driver, driverRole!);
            employeesList.Add(driverEmployee);
        }

        await repository.UnitOfWork.CommitAsync();
        return employeesList;
    }

    private async Task<Employee> TryAddEmployeeAsync(ITenantRepository repository, User user, TenantRole role)
    {
        var employee = await repository.GetAsync<Employee>(user.Id);

        if (employee != null)
            return employee;

        employee = new Employee { Id = user.Id };
        await repository.AddAsync(employee);
        employee.Roles.Add(role);
        _logger.LogInformation("Added an employee {Name} with role {Role}", user.UserName, role.Name);
        return employee;
    }
}