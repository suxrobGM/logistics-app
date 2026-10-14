using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

/// <summary>
///     Get the current tenant's QuickBooks Online connection status and posting settings. Not
///     feature-gated: a tenant whose plan dropped accounting still needs to see the connection to disconnect it.
/// </summary>
public class GetQuickBooksConnectionQuery : IQuery<Result<AccountingConnectionDto>>;
