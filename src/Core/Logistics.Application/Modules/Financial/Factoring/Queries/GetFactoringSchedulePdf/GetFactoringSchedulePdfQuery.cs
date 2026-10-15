using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

public class GetFactoringSchedulePdfQuery : IQuery<Result<InvoicePdfResult>>
{
    public Guid BatchId { get; set; }
}
