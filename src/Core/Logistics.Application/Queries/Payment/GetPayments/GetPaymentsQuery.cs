using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetPaymentsQuery : PagedIntervalQuery, IRequest<PagedResult<PaymentDto>>
{
    /// <summary>
    /// Filter payments by SubscriptionId
    /// </summary>
    public string? SubscriptionId { get; set; }
}
