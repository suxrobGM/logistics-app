using Logistics.Shared.Enums;

namespace Logistics.Shared.Models;

public class PaymentDto
{
    public string Id { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMethod? Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentFor PaymentFor { get; set; }
    public AddressDto? BillingAddress { get; set; }
    public string? Comment { get; set; }
}
