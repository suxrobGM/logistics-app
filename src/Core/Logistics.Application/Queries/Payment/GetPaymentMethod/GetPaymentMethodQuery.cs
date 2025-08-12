using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetPaymentMethodQuery : IAppRequest<Result<PaymentMethodDto>>
{
    public Guid Id { get; set; }
}
