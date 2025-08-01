﻿using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Domain.Primitives.Enums;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdatePaymentCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Description { get; set; }
}
