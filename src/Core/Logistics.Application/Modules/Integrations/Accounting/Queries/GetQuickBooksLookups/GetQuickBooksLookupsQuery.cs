using Logistics.Application.Abstractions.Common;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

/// <summary>Read the connected QuickBooks company's accounts and items for the settings pickers.</summary>
[RequiresFeature(TenantFeature.Accounting)]
public class GetQuickBooksLookupsQuery : IQuery<Result<AccountingLookupsDto>>;
