using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class RenewSubscriptionCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
}
