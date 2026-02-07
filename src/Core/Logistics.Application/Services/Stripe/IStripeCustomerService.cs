using Logistics.Domain.Entities;
using StripeCustomer = Stripe.Customer;

namespace Logistics.Application.Services;

public interface IStripeCustomerService
{
    /// <summary>
    ///     Retrieve a Stripe customer by their ID.
    /// </summary>
    Task<StripeCustomer> GetCustomerAsync(string stripeCustomerId);

    /// <summary>
    ///     Create a new customer in Stripe for the given tenant.
    /// </summary>
    Task<StripeCustomer> CreateCustomerAsync(Tenant tenant);

    /// <summary>
    ///     Update an existing Stripe customer with the tenant's information.
    /// </summary>
    Task<StripeCustomer> UpdateCustomerAsync(Tenant tenant);

    /// <summary>
    ///     Delete a Stripe customer by their ID.
    /// </summary>
    Task DeleteCustomerAsync(string stripeCustomerId);
}
