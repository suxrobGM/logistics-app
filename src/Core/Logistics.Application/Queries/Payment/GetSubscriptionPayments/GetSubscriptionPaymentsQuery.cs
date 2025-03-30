using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetSubscriptionPaymentsQuery : PagedQuery, IRequest<PagedResult<SubscriptionPaymentDto>>
{
    public string SubscriptionId { get; set; } = null!;
}
