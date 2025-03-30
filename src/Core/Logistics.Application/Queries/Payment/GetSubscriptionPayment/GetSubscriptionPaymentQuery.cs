using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetSubscriptionPaymentQuery : IRequest<Result<SubscriptionPaymentDto>>
{
    public string PaymentId { get; set; } = null!;
    public string SubscriptionId { get; set; } = null!;
}
