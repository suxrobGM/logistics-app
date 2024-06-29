using Logistics.Domain.ValueObjects;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdatePaymentCommand : IRequest<Result>
{
    public string Id { get; set; } = default!;
    public PaymentMethod? Method { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentFor? PaymentFor { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Comment { get; set; }
}
