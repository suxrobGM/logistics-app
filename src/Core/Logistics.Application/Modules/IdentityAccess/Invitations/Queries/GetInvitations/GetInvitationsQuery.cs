using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Invitations.Queries;

public class GetInvitationsQuery : SearchableQuery, IQuery<PagedResult<InvitationDto>>
{
    public InvitationStatus? Status { get; set; }
    public InvitationType? Type { get; set; }
}
