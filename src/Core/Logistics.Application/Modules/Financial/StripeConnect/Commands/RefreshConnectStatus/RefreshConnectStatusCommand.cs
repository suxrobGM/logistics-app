using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.StripeConnect.Commands;

/// <summary>
/// Refreshes the Stripe Connect status for the current tenant by syncing from Stripe and
/// persisting the cached fields on the tenant row. Returns the latest status DTO.
/// </summary>
public record RefreshConnectStatusCommand : ICommand<Result<StripeConnectStatusDto>>;
