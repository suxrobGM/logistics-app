using Logistics.Domain.Entities;

namespace Logistics.Application.Abstractions.Payments;

public interface IStripeCustomerService
{
    /// <summary>
    ///     Create a new customer in Stripe for the given tenant.
    /// </summary>
    /// <returns>The Stripe customer ID.</returns>
    Task<string> CreateCustomerAsync(Tenant tenant);

    /// <summary>
    ///     Update an existing Stripe customer with the tenant's information.
    /// </summary>
    Task UpdateCustomerAsync(Tenant tenant);

    /// <summary>
    ///     Delete a Stripe customer by their ID.
    /// </summary>
    Task DeleteCustomerAsync(string stripeCustomerId);
}
