﻿namespace Logistics.Shared.Models;

public class SubscriptionPlanDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? StripePriceId { get; set; }
    public bool HasTrial { get; set; }
}
