using Stripe;

using AddressValueObject = Logistics.Domain.Primitives.ValueObjects.Address;

namespace Logistics.Application.Abstractions.Payments;

/// <summary>
/// Port for converting inbound Stripe address payloads (from webhooks or API responses)
/// into the domain <see cref="AddressValueObject"/>. Lives in Abstractions so Application
/// handlers that process Stripe webhook events do not depend on Infrastructure.Payments.
/// </summary>
public interface IStripeAddressMapper
{
    AddressValueObject ToAddress(Address stripeAddress);
    AddressValueObject ToAddress(AddressOptions addressOptions);
}
