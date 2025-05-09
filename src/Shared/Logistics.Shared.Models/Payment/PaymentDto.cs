﻿using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class PaymentDto
{
    public string Id { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PaymentMethodType? Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentFor PaymentFor { get; set; }
    public AddressDto? BillingAddress { get; set; }
    public string? Notes { get; set; }
    public string? SubscriptionId { get; set; }
}
