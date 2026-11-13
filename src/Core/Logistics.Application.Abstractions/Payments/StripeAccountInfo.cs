namespace Logistics.Application.Abstractions.Payments;

public sealed record StripeAccountInfo(string Id, IReadOnlyDictionary<string, string> Metadata);
