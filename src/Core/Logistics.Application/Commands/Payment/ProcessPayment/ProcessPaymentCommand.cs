﻿using Logistics.Shared.Models;
using Logistics.Domain.Primitives.Enums;
using MediatR;

namespace Logistics.Application.Commands;

public class ProcessPaymentCommand : IRequest<Result>
{
    public Guid PaymentId { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public string? CardholderName { get; set; }
    public string? CardNumber { get; set; }
    public string? CardExpirationDate { get; set; }
    public string? CardCvv { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string BillingAddress { get; set; } = null!;
}
