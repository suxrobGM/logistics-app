using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

public class GetFactoringBatchByIdQuery : IQuery<Result<FactoringBatchDto>>
{
    public Guid Id { get; set; }
}
