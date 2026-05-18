using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Queries;

public class GetPaymentQuery : IQuery<Result<PaymentDto>>
{
    public Guid Id { get; set; }
}
