using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetPaymentQuery : IRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
}
