using Logistics.DbMigrator.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Services;

internal class PopulateFakeData
{
    private const string UserDefaultPassword = "Test12345#";
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random;
    private readonly ITenantRepository _tenantRepository;
    private readonly IMainRepository _mainRepository;
    private readonly IConfiguration _configuration;
    
    public PopulateFakeData(
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _tenantRepository = serviceProvider.GetRequiredService<ITenantRepository>();
        _mainRepository = serviceProvider.GetRequiredService<IMainRepository>();
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
        _random = new Random();
    }
    
    public async Task ExecuteAsync()
    {
        try
        {
            var populate = _configuration.GetValue<bool>("PopulateFakeData");

            if (!populate)
                return;

            _logger.LogInformation("Populating databases with fake data");
            var users = await AddUsersAsync();
            var employees = await AddEmployeesAsync(users);
            var trucks = await AddTrucksAsync(employees.Drivers);
            var customers = await AddCustomersAsync();
            await AddLoadsAsync(employees, trucks, customers);
            await AddNotificationsAsync();
            _logger.LogInformation("Databases have been populated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError("Thrown exception in PopulateData.ExecuteAsync(): {Exception}", ex);
        }
    }
    
    private async Task<IList<User>> AddUsersAsync()
    {
        var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
        var testUsers = _configuration.GetSection("Users").Get<User[]>();
        var usersList = new List<User>();

        if (testUsers == null)
            return usersList;
        
        foreach (var fakeUser in testUsers)
        {
            var user = await userManager.FindByNameAsync(fakeUser.Email!);

            if (user != null)
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

    private async Task<CompanyEmployees> AddEmployeesAsync(IList<User> users)
    {
        if (users.Count < 10)
            throw new InvalidOperationException("Add at least 10 test users in the 'testData.json' under the `Users` section");
        
        var tenant = await _mainRepository.GetAsync<Tenant>(i => i.Name == "default");

        if (tenant is null)
            throw new InvalidOperationException("Could not find the default tenant");
        
        var owner = users[0];
        var manager = users[1];
        var dispatchers = users.Skip(2).Take(3);
        var drivers = users.Skip(5);

        var roles = await _tenantRepository.GetListAsync<TenantRole>();
        var ownerRole = roles.First(i => i.Name == TenantRoles.Owner);
        var managerRole = roles.First(i => i.Name == TenantRoles.Manager);
        var dispatcherRole = roles.First(i => i.Name == TenantRoles.Dispatcher);
        var driverRole = roles.First(i => i.Name == TenantRoles.Driver);

        var ownerEmployee = await TryAddEmployeeAsync(tenant.Id, owner, 0, SalaryType.None, ownerRole);
        var managerEmployee = await TryAddEmployeeAsync(tenant.Id, manager, 5000, SalaryType.Monthly, managerRole);
        var employeesDto = new CompanyEmployees(ownerEmployee, managerEmployee);

        foreach (var dispatcher in dispatchers)
        {
            var dispatcherEmployee = await TryAddEmployeeAsync(tenant.Id, dispatcher, 1000, SalaryType.Weekly, dispatcherRole);
            employeesDto.Dispatchers.Add(dispatcherEmployee);
        }
        
        foreach (var driver in drivers)
        {
            var driverEmployee = await TryAddEmployeeAsync(tenant.Id, driver, 0.3M, SalaryType.ShareOfGross, driverRole);
            employeesDto.Drivers.Add(driverEmployee);
        }

        await _tenantRepository.UnitOfWork.CommitAsync();
        await _mainRepository.UnitOfWork.CommitAsync();
        return employeesDto;
    }

    private async Task<Employee> TryAddEmployeeAsync(
        string tenantId, 
        User user,
        decimal salary,
        SalaryType salaryType,
        TenantRole role)
    {
        var employee = await _tenantRepository.GetAsync<Employee>(user.Id);

        if (employee != null)
            return employee;

        employee = Employee.CreateEmployeeFromUser(user, salary, salaryType);
        user.JoinTenant(tenantId);
        await _tenantRepository.AddAsync(employee);
        employee.Roles.Add(role);
        _logger.LogInformation("Added an employee {Name} with role {Role}", user.UserName, role.Name);
        return employee;
    }

    private async Task<IList<Customer>> AddCustomersAsync()
    {
        var customers = _configuration.GetRequiredSection("Customers").Get<Customer[]>()!;
        var customersList = new List<Customer>();

        foreach (var customer in customers)
        {
            var existingCustomer = await _tenantRepository.GetAsync<Customer>(i => i.Name == customer.Name);
            customersList.Add(customer);
            
            if (existingCustomer is not null)
                continue;

            await _tenantRepository.AddAsync(customer);
            _logger.LogInformation("Added a customer '{CustomerName}'", customer.Name);
        }

        await _tenantRepository.UnitOfWork.CommitAsync();
        return customersList;
    }

    private async Task<IList<Truck>> AddTrucksAsync(IEnumerable<Employee> drivers)
    {
        var trucksList = new List<Truck>();
        var truckNumber = 101;

        foreach (var driver in drivers)
        {
            var truck = driver.Truck;
            
            if (truck != null)
            {
                trucksList.Add(truck);
                continue;
            }

            truck = Truck.Create(truckNumber.ToString(), driver);
            truckNumber++;
            trucksList.Add(truck);
            await _tenantRepository.AddAsync(truck);
            _logger.LogInformation("Added a truck {Number}", truck.TruckNumber);
        }

        await _tenantRepository.UnitOfWork.CommitAsync();
        return trucksList;
    }

    private async Task AddLoadsAsync(CompanyEmployees companyEmployees, IList<Truck> trucks, IList<Customer> customers)
    {
        if (!trucks.Any())
            throw new InvalidOperationException("Empty list of trucks");
        
        var loadsDb = await _tenantRepository.GetListAsync<Load>();

        for (ulong i = 1; i <= 100; i++)
        {
            var refId = 1000 + i;
            var existingLoad = loadsDb.FirstOrDefault(m => m.RefId == refId);

            if (existingLoad != null)
                continue;

            var truck = _random.Pick(trucks);
            var customer = _random.Pick(customers);
            var dispatcher = _random.Pick(companyEmployees.Dispatchers);
            await AddLoadAsync(i, truck, dispatcher, customer);
        }

        await _tenantRepository.UnitOfWork.CommitAsync();
    }

    private async Task AddLoadAsync(
        ulong index,
        Truck truck,
        Employee dispatcher,
        Customer customer)
    {
        var dispatchedDate = _random.Date(DateTime.Today.AddMonths(-3), DateTime.Today.AddDays(-3));
        const string originAddress = "40 Crescent Ave, Boston, MA 02125, United States";
        const double originLat = 42.319090;
        const double originLng = -71.054680;
        const string destAddress = "73 Tremont St, Boston, MA 02108, United States";
        const double destLat = 42.357820;
        const double destLng = -71.060810;
            
        var load = Load.Create(
            1000 + index, 
            originAddress, 
            originLat,
            originLng,
            destAddress, 
            destLat,
            destLng,
            customer,
            truck, 
            dispatcher);
        
        load.Name = $"Test cargo {index}";
        load.DispatchedDate = dispatchedDate;
        load.PickUpDate = dispatchedDate.AddDays(1);
        load.DeliveryDate = dispatchedDate.AddDays(2);
        load.Distance = _random.Next(16093, 321869);
        load.DeliveryCost = _random.Next(1000, 3000);
        load.Invoice = CreateInvoice(load);

        await _tenantRepository.AddAsync(load);
        _logger.LogInformation("Added a load {Name}", load.Name);
    }

    private Invoice CreateInvoice(Load load)
    {
        const string companyName = "Test Company";
        const string companyAddress = "7 Allstate Rd, Dorchester, MA 02125, United States";
        
        var payment = new Payment
        {
            Amount = load.DeliveryCost,
            Status = PaymentStatus.Paid,
            CreatedDate = DateTime.Today,
            PaymentDate = DateTime.Now,
            PaymentFor = PaymentFor.Invoice,
            Method = PaymentMethod.BankAccount
        };

        var invoice = new Invoice
        {
            CompanyName = companyName,
            CompanyAddress = companyAddress,
            CustomerId = load.CustomerId!,
            Customer = load.Customer!,
            LoadId = load.Id,
            Load = load,
            PaymentId = payment.Id,
            Payment = payment,
        };

        return invoice;
    }

    private async Task AddNotificationsAsync()
    {
        var notificationsCount = await _tenantRepository.CountAsync<Notification>();

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
                CreatedDate = _random.Date(DateTime.Today.AddMonths(-1), DateTime.Today.AddDays(-1))
            };

            await _tenantRepository.AddAsync(notification);
            _logger.LogInformation("Added a notification {Notification}", notification.Title);
        }

        await _tenantRepository.UnitOfWork.CommitAsync();
    }
}

internal record CompanyEmployees(Employee Owner, Employee Manager)
{
    public IList<Employee> Dispatchers { get; } = new List<Employee>();
    public IList<Employee> Drivers { get; } = new List<Employee>();
}
