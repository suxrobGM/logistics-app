using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.StripeConnect.Commands;

/// <summary>
/// Creates a Stripe Connect Express account for the current tenant.
/// </summary>
public record CreateConnectAccountCommand : ICommand<Result<CreateConnectAccountDto>>;
