namespace Logistics.HttpClient.Models;

public class UpdateSubscriptionPlan
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
}
