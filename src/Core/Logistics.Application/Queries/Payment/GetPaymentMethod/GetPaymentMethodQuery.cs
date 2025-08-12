using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetPaymentMethodQuery : IRequest<Result<PaymentMethodDto>>
{
    public Guid Id { get; set; }
}
