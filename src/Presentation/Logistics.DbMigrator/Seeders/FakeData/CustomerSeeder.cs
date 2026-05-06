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

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var region = context.Region?.Region.ToString() ?? throw new InvalidOperationException("Region not set");
        var customers = context.Configuration.GetRequiredSection($"{region}:Customers").Get<Customer[]>()!;
        var customersList = new List<Customer>();
        var customerRepository = context.TenantUnitOfWork.Repository<Customer>();

        foreach (var customer in customers)
        {
            var existingCustomer = await customerRepository.GetAsync(i => i.Name == customer.Name, cancellationToken);

            if (existingCustomer is not null)
            {
                BackfillMissingSeedData(existingCustomer, customer);
                customersList.Add(existingCustomer);
                continue;
            }

            customersList.Add(customer);
            await customerRepository.AddAsync(customer, cancellationToken);
            logger.LogInformation("Created customer '{CustomerName}'", customer.Name);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        context.CreatedCustomers = customersList;
        LogCompleted(customersList.Count);
    }

    private static void BackfillMissingSeedData(Customer existingCustomer, Customer seedCustomer)
    {
        existingCustomer.Address ??= seedCustomer.Address;
        existingCustomer.Email ??= seedCustomer.Email;
        existingCustomer.Phone ??= seedCustomer.Phone;
        existingCustomer.Notes ??= seedCustomer.Notes;
        existingCustomer.TaxId ??= seedCustomer.TaxId;

        if (seedCustomer.IsVatExempt)
        {
            existingCustomer.IsVatExempt = true;
        }
    }
}
