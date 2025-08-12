using System.Text.Json.Serialization;

using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record TenantDto
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public string? BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public Address? CompanyAddress { get; set; }
    public string? StripeCustomerId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ConnectionString { get; set; }

    public SubscriptionDto? Subscription { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EmployeeCount { get; set; }
}
