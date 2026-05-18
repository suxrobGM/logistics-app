using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

public class CreatePaymentCommand : ICommand
{
    public string? StripePaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Description { get; set; }
}
