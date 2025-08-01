using Logistics.Shared.Models;
using Logistics.Domain.Primitives.Enums;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateLoadInvoiceCommand : IRequest<Result>
{
    public Guid CustomerId { get; set; }
    public Guid LoadId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal PaymentAmount { get; set; }
}
