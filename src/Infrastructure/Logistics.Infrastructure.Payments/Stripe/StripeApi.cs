using Logistics.Application.Abstractions.Payments;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal static class StripeApi
{
    /// <summary>Rethrows <see cref="StripeException"/> as the SDK-free <see cref="StripeApiException"/> the ports promise.</summary>
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
