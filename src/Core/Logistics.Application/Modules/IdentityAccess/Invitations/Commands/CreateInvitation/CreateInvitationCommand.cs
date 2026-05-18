using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Commands;

public class CreateInvitationCommand : ICommand<Result<InvitationDto>>
{
    public required string Email { get; set; }
    public required InvitationType Type { get; set; }
    public required string TenantRole { get; set; }
    public Guid? CustomerId { get; set; }
    public string? PersonalMessage { get; set; }
    public int ExpirationDays { get; set; } = 7;
}
