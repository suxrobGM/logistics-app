namespace Logistics.Shared.Models;

public class SubscriptionPaymentDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PaymentDto Payment { get; set; } = default!;
}
