using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.AISettings.Queries;

public sealed class GetTenantQuotaUsagesQuery : PagedQuery, IQuery<PagedResult<TenantQuotaUsageDto>>;
