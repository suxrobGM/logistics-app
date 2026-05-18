using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

public sealed class GetTenantQuotaUsagesQuery : PagedQuery, IQuery<PagedResult<TenantQuotaUsageDto>>;
