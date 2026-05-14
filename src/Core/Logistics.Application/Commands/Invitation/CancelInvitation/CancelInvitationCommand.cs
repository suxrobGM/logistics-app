using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CancelInvitationCommand : ICrossDatabaseCommand<Result>
{
    public Guid Id { get; set; }
}
