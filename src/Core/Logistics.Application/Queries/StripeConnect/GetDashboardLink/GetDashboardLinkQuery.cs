using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Gets the Stripe Express dashboard link for the current tenant's connected account.
/// </summary>
public record GetDashboardLinkQuery : IAppRequest<Result<DashboardLinkDto>>;
