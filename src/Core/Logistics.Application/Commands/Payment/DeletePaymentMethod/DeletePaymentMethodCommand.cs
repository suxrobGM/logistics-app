using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class DeletePaymentMethodCommand : IRequest<Result>
{
    public required string Id { get; set; }
    public required string TenantId { get; set; }
}
