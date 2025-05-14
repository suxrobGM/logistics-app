using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record PaymentDto
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; } 
    
    public MoneyDto Amount { get; set; } = null!;
    //public PaymentMethodDto? Method { get; set; }
    public Guid MethodId { get; set; }
    public Guid TenantId { get; set; }
    public PaymentStatus Status { get; set; }
    public AddressDto? BillingAddress { get; set; }
    public string? Description { get; set; }
}
