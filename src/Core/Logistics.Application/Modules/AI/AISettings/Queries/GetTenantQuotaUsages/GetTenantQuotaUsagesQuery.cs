using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AISettings.Queries;

public sealed class GetTenantQuotaUsagesQuery : PagedQuery, IQuery<PagedResult<TenantQuotaUsageDto>>;
