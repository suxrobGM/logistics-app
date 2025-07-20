using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;
using Logistics.Shared.Consts.Roles;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Workers;

internal class FakeDataWorker : IHostedService
{
    private const string UserDefaultPassword = "Test12345#";
    private readonly DateTime _startDate = DateTime.UtcNow.AddMonths(-3);
    private readonly DateTime _endDate = DateTime.UtcNow.AddDays(-3); 
    private readonly Random _random = new();
    
    private readonly ILogger<FakeDataWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public FakeDataWorker(
        ILogger<FakeDataWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var payrollService = scope.ServiceProvider.GetRequiredService<PayrollService>();
        var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnityOfWork>();
        var populateFakeDataEnabled = scope.ServiceProvider
            .GetRequiredService<IConfiguration>()
            .GetValue<bool>("PopulateFakeData");

        if (!populateFakeDataEnabled)
        {
            _logger.LogInformation("PopulateFakeData is set to false. Skipping data population");
            return;
        }
        
        // Don't populate fake data if there are already employees in the database
        // In Aspire, this will be called multiple times, so we need to check if there are already employees
        // to avoid duplicating data
        var hasEmployees = tenantUow.Repository<Employee>().Query().Any();
        
        if (hasEmployees)
        {
            _logger.LogInformation("There are already employees in the database. Skipping data population");
            return;
        }
        
        _logger.LogInformation("Populating databases with fake data");
        var users = await AddUsersAsync(scope.ServiceProvider);
        var employees = await AddEmployeesAsync(scope.ServiceProvider, users);
        var trucks = await AddTrucksAsync(scope.ServiceProvider, employees.Drivers);
        var customers = await AddCustomersAsync(scope.ServiceProvider);
        await AddGeneralFreightLoadsAsync(scope.ServiceProvider, employees, trucks, customers);
        await AddCarHaulerTripsAsync(scope.ServiceProvider, employees, trucks, customers);
        await AddNotificationsAsync(scope.ServiceProvider);
        
        await payrollService.GeneratePayrolls(employees, _startDate, _endDate);
        _logger.LogInformation("Databases have been populated successfully");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    private async Task<IList<User>> AddUsersAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        
        var testUsers = configuration.GetSection("Users").Get<User[]>();
        var usersList = new List<User>();

        if (testUsers is null)
        {
            return usersList;
        }
        
        foreach (var fakeUser in testUsers)
        {
            var user = await userManager.FindByNameAsync(fakeUser.Email!);

            if (user is not null)
            {
                usersList.Add(user);
                continue;
            }
            
            user = new User
            {
                UserName = fakeUser.Email,
                FirstName = fakeUser.FirstName,
                LastName = fakeUser.LastName,
                Email = fakeUser.Email,
                EmailConfirmed = true
            };
            
            try
            {
                var result = await userManager.CreateAsync(user, UserDefaultPassword);
                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);
                
                usersList.Add(user);
            }
            finally
            {
                _logger.LogInformation("Created an user {FirstName} {LastName}", fakeUser.FirstName, fakeUser.LastName);
            }
        }
        
        return usersList;
    }

    private async Task<CompanyEmployees> AddEmployeesAsync(IServiceProvider serviceProvider, IList<User> users)
    {
        var masterUow = serviceProvider.GetRequiredService<IMasterUnityOfWork>();
        var tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        
        var tenant = await masterUow.Repository<Tenant>().GetAsync(i => i.Name == "default");

        if (tenant is null)
        {
            throw new InvalidOperationException("Could not find the default tenant");
        }
        
        var owner = users[0];
        var manager = users[1];
        var dispatchers = users.Skip(2).Take(3);
        var drivers = users.Skip(5);

        var roles = await tenantUow.Repository<TenantRole>().GetListAsync();
        var ownerRole = roles.First(i => i.Name == TenantRoles.Owner);
        var managerRole = roles.First(i => i.Name == TenantRoles.Manager);
        var dispatcherRole = roles.First(i => i.Name == TenantRoles.Dispatcher);
        var driverRole = roles.First(i => i.Name == TenantRoles.Driver);

        var ownerEmployee = await TryAddEmployeeAsync(tenantUow, tenant.Id, owner, 0, SalaryType.None, ownerRole);
        var managerEmployee = await TryAddEmployeeAsync(tenantUow, tenant.Id, manager, 5000, SalaryType.Monthly, managerRole);
        var employeesDto = new CompanyEmployees(ownerEmployee, managerEmployee);

        foreach (var dispatcher in dispatchers)
        {
            var dispatcherEmployee = await TryAddEmployeeAsync(tenantUow, tenant.Id, dispatcher, 1000, SalaryType.Weekly, dispatcherRole);
            employeesDto.Dispatchers.Add(dispatcherEmployee);
            employeesDto.AllEmployees.Add(dispatcherEmployee);
        }
        
        foreach (var driver in drivers)
        {
            var driverEmployee = await TryAddEmployeeAsync(tenantUow, tenant.Id, driver, 0.3M, SalaryType.ShareOfGross, driverRole);
            employeesDto.Drivers.Add(driverEmployee);
            employeesDto.AllEmployees.Add(driverEmployee);
        }

        employeesDto.AllEmployees.Add(ownerEmployee);
        employeesDto.AllEmployees.Add(managerEmployee);
        await tenantUow.SaveChangesAsync();
        await masterUow.SaveChangesAsync();
        return employeesDto;
    }

    private async Task<Employee> TryAddEmployeeAsync(
        ITenantUnityOfWork tenantUow,
        Guid tenantId, 
        User user,
        decimal salary,
        SalaryType salaryType,
        TenantRole role)
    {
        var employeeRepository = tenantUow.Repository<Employee>();
        var employee = await employeeRepository.GetByIdAsync(user.Id);

        if (employee is not null)
        {
            return employee;
        }

        employee = Employee.CreateEmployeeFromUser(user, salary, salaryType);
        user.TenantId = tenantId;
        await employeeRepository.AddAsync(employee);
        employee.Roles.Add(role);
        _logger.LogInformation("Added an employee {Name} with role {Role}", user.UserName, role.Name);
        return employee;
    }

    private async Task<IList<Customer>> AddCustomersAsync(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        
        var customers = configuration.GetRequiredSection("Customers").Get<Customer[]>()!;
        var customersList = new List<Customer>();
        var customerRepository = tenantUow.Repository<Customer>();

        foreach (var customer in customers)
        {
            var existingCustomer = await customerRepository.GetAsync(i => i.Name == customer.Name);
            customersList.Add(customer);
            
            if (existingCustomer is not null)
                continue;

            await customerRepository.AddAsync(customer);
            _logger.LogInformation("Added a customer '{CustomerName}'", customer.Name);
        }

        await tenantUow.SaveChangesAsync();
        return customersList;
    }

    private async Task<IList<Truck>> AddTrucksAsync(IServiceProvider serviceProvider, IEnumerable<Employee> drivers)
    {
        var tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        
        var trucksList = new List<Truck>();
        var truckNumber = 101;
        var truckRepository = tenantUow.Repository<Truck>();

        foreach (var driver in drivers)
        {
            var truck = driver.Truck;
            
            if (truck != null)
            {
                trucksList.Add(truck);
                continue;
            }

            var truckType = _random.Pick([TruckType.CarHauler, TruckType.FreightTruck]);
            truck = Truck.Create(truckNumber.ToString(), truckType, driver);
            truckNumber++;
            trucksList.Add(truck);
            await truckRepository.AddAsync(truck);
            _logger.LogInformation("Added a truck with number {Number}, and type {Type}", truck.Number, truck.Type);
        }

        await tenantUow.SaveChangesAsync();
        return trucksList;
    }

    private async Task AddGeneralFreightLoadsAsync(
        IServiceProvider serviceProvider,
        CompanyEmployees companyEmployees, 
        IList<Truck> trucks, 
        IList<Customer> customers)
    {
        if (!trucks.Any())
            throw new InvalidOperationException("Empty list of trucks");
        
        var tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        var dryVanTrucks = trucks
            .Where(i => i.Type == TruckType.FreightTruck)
            .ToList();

        for (long i = 1; i <= 100; i++)
        {
            var truck = _random.Pick(dryVanTrucks);
            var customer = _random.Pick(customers);
            var dispatcher = _random.Pick(companyEmployees.Dispatchers);
            var load = BuildRandomLoad(i, LoadType.GeneralFreight, truck, dispatcher, customer);
            await tenantUow.Repository<Load>().AddAsync(load);
            _logger.LogInformation("Added Load {LoadName} for Truck {TruckNumber}", load.Name, truck.Number);
        }

        await tenantUow.SaveChangesAsync();
    }
    
    private Load BuildRandomLoad(
        long index,
        LoadType loadType,
        Truck truck,
        Employee dispatcher,
        Customer customer)
    {
        const double originLat = 42.319090, originLng = -71.054680;
        const double destLat   = 42.357820, destLng   = -71.060810;

        var dispatchedDate = _random.Date(_startDate, _endDate);
        dispatchedDate = DateTime.SpecifyKind(dispatchedDate, DateTimeKind.Utc);

        var originAddress = new Address
        {
            Line1 = "40 Crescent Ave",
            City = "Boston",
            State = "Massachusetts",
            ZipCode = "02125",
            Country = "United States"
        };

        var destinationAddress = new Address
        {
            Line1 = "73 Tremont St",
            City = "Boston",
            State = "Massachusetts",
            ZipCode = "02108",
            Country = "United States"
        };

        var deliveryCost = _random.Next(1_000, 3_000);
        var loadName = loadType == LoadType.Vehicle
            ? "Car Hauler Load"
            : "Freight Truck Load";

        var load = Load.Create(
            $"{loadName} {index}",
            loadType,
            deliveryCost,
            originAddress,  originLat, originLng,
            destinationAddress, destLat, destLng,
            customer,
            truck,
            dispatcher);

        load.DispatchedDate = dispatchedDate;
        load.PickUpDate = dispatchedDate.AddDays(1);
        load.DeliveryDate = dispatchedDate.AddDays(2);
        load.Distance = _random.Next(16_093, 321_869);   // 10–200 mi
        return load;
    }
    
    private async Task AddCarHaulerTripsAsync(
        IServiceProvider serviceProvider,
        CompanyEmployees employees,
        IList<Truck> trucks,
        IList<Customer> customers)
    {
        var tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        var tripRepo = tenantUow.Repository<Trip>();
        var loadRepo = tenantUow.Repository<Load>();

        long tripNr = 1;

        for (var i = 0; i < 30; i++) // ➜ 30 test trips
        {
            var truck = _random.Pick(trucks);
            var dispatcher = _random.Pick(employees.Dispatchers);
            var customer = _random.Pick(customers);

            var planned = _random.Date(_startDate, _endDate);
            planned = DateTime.SpecifyKind(planned, DateTimeKind.Utc);

            // create 1-4 loads that belong to this trip
            var loadsCount = _random.Next(1, 5);
            var loads = new List<Load>();

            for (var j = 0; j < loadsCount; j++)
            {
                var load = BuildRandomLoad(i * 10 + j + 1, LoadType.Vehicle, truck, dispatcher, customer);
                loads.Add(load);
                await loadRepo.AddAsync(load); // persist each load
            }

            // Trip factory builds stops automatically
            var trip = Trip.Create(
                name: $"Trip {tripNr}",
                plannedStart: planned,
                truck: truck,
                loads: loads);

            tripNr++;
            await tripRepo.AddAsync(trip);

            _logger.LogInformation("Added Trip {TripName} with {Count} loads", trip.Name, loadsCount);
        }

        await tenantUow.SaveChangesAsync();
    }


    private async Task AddNotificationsAsync(IServiceProvider serviceProvider)
    {
        var tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        
        var notificationRepository = tenantUow.Repository<Notification>();
        var notificationsCount = await notificationRepository.CountAsync();

        if (notificationsCount > 0)
        {
            return;
        }
        
        for (var i = 1; i <= 10; i++)
        {
            var notification = new Notification
            {
                Title = $"Test notification {i}",
                Message = $"Notification {i} description",
                CreatedDate = DateTime.SpecifyKind(_random.Date(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddDays(-1)), DateTimeKind.Utc)
            };

            await notificationRepository.AddAsync(notification);
            _logger.LogInformation("Added a notification {Notification}", notification.Title);
        }

        await tenantUow.SaveChangesAsync();
    }
}
