using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Queries;

public class GetPaymentMethodsQuery : IRequest<Result<PaymentMethodDto[]>>
{
    public string? OrderBy { get; set; }
}
