using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

public class GetFactoringBatchesQuery : PagedQuery, IQuery<PagedResult<FactoringBatchDto>>
{
    public FactoringBatchStatus? Status { get; set; }
    public Guid? FactoringCompanyId { get; set; }
}
