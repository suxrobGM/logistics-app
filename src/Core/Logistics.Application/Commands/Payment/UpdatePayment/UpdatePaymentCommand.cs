using Logistics.Domain.ValueObjects;
using Logistics.Shared;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdatePaymentCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public PaymentMethod? Method { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentFor? PaymentFor { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Comment { get; set; }
}
