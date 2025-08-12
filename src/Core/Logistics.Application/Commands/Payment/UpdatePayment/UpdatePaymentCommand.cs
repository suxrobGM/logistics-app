using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Commands;

public class UpdatePaymentCommand : IAppRequest
{
    public Guid Id { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Description { get; set; }
}
