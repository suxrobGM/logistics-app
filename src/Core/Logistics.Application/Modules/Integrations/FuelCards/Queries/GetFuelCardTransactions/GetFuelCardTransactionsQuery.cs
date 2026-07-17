using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

public class GetFuelCardTransactionsQuery : PagedQuery, IQuery<PagedResult<FuelCardTransactionDto>>
{
    public FuelCardTransactionStatus? Status { get; set; }
    public FuelCardProviderType? ProviderType { get; set; }
    public Guid? TruckId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
