using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Gets the current Stripe Connect status for the tenant.
/// </summary>
public record GetConnectStatusQuery : IAppRequest<Result<StripeConnectStatusDto>>;
