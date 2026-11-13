using Logistics.Application.Abstractions.Common;
using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

/// <summary>Queue an immediate QuickBooks sync for the current tenant ("Sync now").</summary>
[RequiresFeature(TenantFeature.Accounting)]
public class SyncQuickBooksCommand : ICommand;
