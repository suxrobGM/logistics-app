using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Gets the Stripe Connect onboarding link for the current tenant.
/// </summary>
public record GetOnboardingLinkQuery(string ReturnUrl, string RefreshUrl) : IAppRequest<Result<string>>;
