using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetPaymentMethodsQuery : IAppRequest<Result<PaymentMethodDto[]>>
{
    public string? OrderBy { get; set; }
}
