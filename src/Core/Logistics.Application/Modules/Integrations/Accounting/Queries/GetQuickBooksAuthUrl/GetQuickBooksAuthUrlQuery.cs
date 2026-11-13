using Logistics.Application.Abstractions.Common;
using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

/// <summary>
///     Build the QuickBooks Online OAuth2 authorization URL for the current tenant. The frontend
///     redirects the browser to the returned URL to begin the consent flow.
/// </summary>
[RequiresFeature(TenantFeature.Accounting)]
public class GetQuickBooksAuthUrlQuery : IQuery<Result<AccountingAuthUrlDto>>;
