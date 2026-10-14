using Logistics.Application.Abstractions.Common;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

/// <summary>
///     Retry failed QuickBooks pushes now instead of after the hourly back-off, and queue a sync.
///     With <see cref="Id"/> only that record is retried; without it every failed record is.
/// </summary>
[RequiresFeature(TenantFeature.Accounting)]
public class RetryQuickBooksSyncIssuesCommand : ICommand
{
    public Guid? Id { get; set; }
}
