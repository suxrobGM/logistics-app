using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetPaymentQuery : IAppRequest<Result<PaymentDto>>
{
    public Guid Id { get; set; }
}
