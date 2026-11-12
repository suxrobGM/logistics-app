using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Services;

public static class CustomerUnitOfWorkExtensions
{
    extension(ITenantUnitOfWork tenantUow)
    {
        /// <summary>
        ///     Finds the customer whose name matches ignoring case, or adds a new one. The caller saves;
        ///     <c>Created</c> tells it whether there is a new row to save.
        /// </summary>
        public async Task<(Customer Customer, bool Created)> FindOrAddCustomerAsync(string name, CancellationToken ct)
        {
            var lowerName = name.ToLower();
            var existing = await tenantUow.Repository<Customer>().GetAsync(c => c.Name.ToLower() == lowerName, ct);
            if (existing is not null)
            {
                return (existing, false);
            }

            var customer = new Customer { Name = name };
            await tenantUow.Repository<Customer>().AddAsync(customer, ct);
            return (customer, true);
        }
    }
}
