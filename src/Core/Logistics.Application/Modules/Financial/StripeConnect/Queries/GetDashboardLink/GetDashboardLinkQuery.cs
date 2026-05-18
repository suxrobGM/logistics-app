using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.StripeConnect.Queries;

/// <summary>
/// Gets the Stripe Express dashboard link for the current tenant's connected account.
/// </summary>
public record GetDashboardLinkQuery : IQuery<Result<DashboardLinkDto>>;
