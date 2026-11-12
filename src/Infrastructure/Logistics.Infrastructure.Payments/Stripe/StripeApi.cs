using Logistics.Application.Abstractions.Payments.Stripe;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal static class StripeApi
{
    /// <summary>
    /// Runs a Stripe SDK call and rethrows its <see cref="StripeException"/> as the SDK-free
    /// <see cref="StripeApiException"/> that the ports promise their callers.
    /// </summary>
    public static async Task<T> CallAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return await call();
        }
        catch (StripeException ex)
        {
            throw new StripeApiException(ex.Message, ex.StripeError?.Code, ex);
        }
    }
}
