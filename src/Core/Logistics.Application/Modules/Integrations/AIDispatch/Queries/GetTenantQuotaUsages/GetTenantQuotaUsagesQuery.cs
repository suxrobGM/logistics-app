using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

public sealed class GetTenantQuotaUsagesQuery : PagedQuery, IQuery<PagedResult<TenantQuotaUsageDto>>;
