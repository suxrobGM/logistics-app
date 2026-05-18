using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Queries;

public class GetInvitationByIdQuery : IQuery<Result<InvitationDto>>
{
    public Guid Id { get; set; }
}
