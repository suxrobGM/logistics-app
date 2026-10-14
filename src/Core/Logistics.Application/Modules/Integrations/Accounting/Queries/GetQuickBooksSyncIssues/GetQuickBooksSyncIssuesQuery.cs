using Logistics.Application.Abstractions.Common;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

/// <summary>The most recent records that failed to push to QuickBooks, with the error QuickBooks gave.</summary>
[RequiresFeature(TenantFeature.Accounting)]
public class GetQuickBooksSyncIssuesQuery : IQuery<Result<List<AccountingSyncIssueDto>>>;
