using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class ResendInvitationCommand : ICommand<Result>
{
    public Guid Id { get; set; }
}
