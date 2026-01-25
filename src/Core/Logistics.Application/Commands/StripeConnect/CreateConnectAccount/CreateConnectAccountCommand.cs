using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Creates a Stripe Connect Express account for the current tenant.
/// </summary>
public record CreateConnectAccountCommand : IAppRequest<Result<CreateConnectAccountDto>>;
