using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record PaymentDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public PaymentMethodType? Method { get; set; }
    public MoneyDto Amount { get; set; } = null!;
    public PaymentStatus Status { get; set; }
    public AddressDto? BillingAddress { get; set; }
}
