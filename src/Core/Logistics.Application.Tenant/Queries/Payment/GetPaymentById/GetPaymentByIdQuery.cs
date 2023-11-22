using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetPaymentByIdQuery : IRequest<ResponseResult<PaymentDto>>
{
    public string Id { get; set; } = default!;
}
