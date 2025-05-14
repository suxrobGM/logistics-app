using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;
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
