using Logistics.Shared.Models;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetPaymentByIdQuery : IRequest<Result<PaymentDto>>
{
    public string Id { get; set; } = null!;
}
