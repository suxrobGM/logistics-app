using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

/// <summary>
///     Disconnect the current tenant's QuickBooks Online integration: revoke the token at the
///     provider and deactivate the stored configuration. Deliberately not feature-gated, so a tenant
///     whose plan no longer includes accounting can still revoke the access it granted.
/// </summary>
public class DisconnectQuickBooksCommand : ICommand;
