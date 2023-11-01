using Logistics.Shared.Enums;

namespace Logistics.Shared.Models;

public class PaymentDto
{
    public DateTime CreatedDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMethod? Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentFor PaymentFor { get; set; }
    public string? Comment { get; set; }
}
