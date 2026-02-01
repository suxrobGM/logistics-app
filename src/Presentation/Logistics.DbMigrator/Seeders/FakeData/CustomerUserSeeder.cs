using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Seeders.Infrastructure;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds customer portal users from fake-dataset.json configuration.
/// Creates User, CustomerUser, and UserTenantAccess entities.
/// </summary>
internal class CustomerUserSeeder(ILogger<CustomerUserSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(CustomerUserSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 125;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(CustomerSeeder), nameof(UserSeeder), nameof(DefaultTenantSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<CustomerUser>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var portalUsers = context.Configuration.GetSection("CustomerPortalUsers").Get<CustomerPortalUserData[]>();
        if (portalUsers is null || portalUsers.Length == 0)
        {
            logger.LogWarning("No customer portal users found in configuration");
            LogCompleted(0);
            return;
        }

        var customers = context.CreatedCustomers ?? throw new InvalidOperationException("Customers not seeded");
        var tenant = context.DefaultTenant ?? throw new InvalidOperationException("Default tenant not seeded");

        var customerUserRepository = context.TenantUnitOfWork.Repository<CustomerUser>();
        var tenantAccessRepository = context.MasterUnitOfWork.Repository<UserTenantAccess>();
        var customerUsersList = new List<CustomerUser>();
        var createdTenantAccessKeys = new HashSet<(Guid UserId, Guid TenantId)>();

        foreach (var portalUserData in portalUsers)
        {
            // Create or get the User
            var user = await context.UserManager.FindByNameAsync(portalUserData.Email);
            if (user is null)
            {
                user = new User
                {
                    UserName = portalUserData.Email,
                    FirstName = portalUserData.FirstName,
                    LastName = portalUserData.LastName,
                    Email = portalUserData.Email,
                    EmailConfirmed = true,
                    TenantId = tenant.Id
                };

                var result = await context.UserManager.CreateAsync(user, portalUserData.Password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create portal user {portalUserData.Email}: {result.Errors.First().Description}");
                }

                logger.LogInformation("Created portal user {Email}", portalUserData.Email);
            }

            // Create CustomerUser for each associated customer
            foreach (var customerName in portalUserData.CustomerNames)
            {
                var customer = customers.FirstOrDefault(c => c.Name == customerName);
                if (customer is null)
                {
                    logger.LogWarning("Customer '{CustomerName}' not found for portal user {Email}",
                        customerName, portalUserData.Email);
                    continue;
                }

                // Check if CustomerUser already exists
                var existingCustomerUser = await customerUserRepository.GetAsync(
                    cu => cu.UserId == user.Id && cu.CustomerId == customer.Id,
                    cancellationToken);

                CustomerUser customerUser;
                if (existingCustomerUser is null)
                {
                    customerUser = new CustomerUser
                    {
                        UserId = user.Id,
                        CustomerId = customer.Id,
                        Email = portalUserData.Email,
                        DisplayName = $"{portalUserData.FirstName} {portalUserData.LastName}",
                        IsActive = true
                    };

                    await customerUserRepository.AddAsync(customerUser, cancellationToken);
                    logger.LogInformation("Created CustomerUser linking {Email} to {CustomerName}",
                        portalUserData.Email, customerName);
                }
                else
                {
                    customerUser = existingCustomerUser;
                }

                customerUsersList.Add(customerUser);

                // Create UserTenantAccess in master database (only once per user+tenant)
                var accessKey = (user.Id, tenant.Id);
                if (!createdTenantAccessKeys.Contains(accessKey))
                {
                    var existingAccess = await tenantAccessRepository.GetAsync(
                        uta => uta.UserId == user.Id && uta.TenantId == tenant.Id,
                        cancellationToken);

                    if (existingAccess is null)
                    {
                        var tenantAccess = new UserTenantAccess
                        {
                            UserId = user.Id,
                            TenantId = tenant.Id,
                            CustomerUserId = customerUser.Id,
                            CustomerName = customer.Name,
                            IsActive = true
                        };

                        await tenantAccessRepository.AddAsync(tenantAccess, cancellationToken);
                        logger.LogInformation("Created UserTenantAccess for {Email} to tenant {TenantName}",
                            portalUserData.Email, tenant.Name);
                    }

                    createdTenantAccessKeys.Add(accessKey);
                }
            }
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);

        context.CreatedCustomerUsers = customerUsersList;
        LogCompleted(customerUsersList.Count);
    }
}
