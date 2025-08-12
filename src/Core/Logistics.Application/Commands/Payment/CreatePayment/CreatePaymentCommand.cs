using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Commands;

public class CreatePaymentCommand : IAppRequest
{
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Description { get; set; }
}
