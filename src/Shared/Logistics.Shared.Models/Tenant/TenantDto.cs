using System.Text.Json.Serialization;

namespace Logistics.Shared.Models;

public class TenantDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public AddressDto CompanyAddress { get; set; } = AddressDto.Empty();
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ConnectionString { get; set; }
    
    public SubscriptionDto? Subscription { get; set; }
}
