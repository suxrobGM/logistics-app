using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreatePaymentCommand : IRequest<Result>
{
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Description { get; set; }
}
