using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds customers from fake-dataset.json configuration.
/// </summary>
internal class CustomerSeeder(ILogger<CustomerSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(CustomerSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 120;

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Customer>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var customers = context.Configuration.GetRequiredSection("Customers").Get<Customer[]>()!;
        var customersList = new List<Customer>();
        var customerRepository = context.TenantUnitOfWork.Repository<Customer>();

        foreach (var customer in customers)
        {
            var existingCustomer = await customerRepository.GetAsync(i => i.Name == customer.Name, cancellationToken);
            customersList.Add(existingCustomer ?? customer);

            if (existingCustomer is not null)
            {
                continue;
            }

            await customerRepository.AddAsync(customer, cancellationToken);
            Logger.LogInformation("Created customer '{CustomerName}'", customer.Name);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        context.CreatedCustomers = customersList;
        LogCompleted(customersList.Count);
    }
}
