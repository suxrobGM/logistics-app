using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class RenewSubscriptionCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
