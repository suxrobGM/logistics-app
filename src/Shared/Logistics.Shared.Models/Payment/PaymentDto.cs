using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record PaymentDto
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }

    public decimal Amount { get; set; }
    public string? StripePaymentMethodId { get; set; }
    public PaymentStatus Status { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Description { get; set; }
}
