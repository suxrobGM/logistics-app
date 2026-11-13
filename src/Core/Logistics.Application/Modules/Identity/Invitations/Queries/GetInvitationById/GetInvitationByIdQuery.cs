using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Identity.Invitations.Queries;

public class GetInvitationByIdQuery : IQuery<Result<InvitationDto>>
{
    public Guid Id { get; set; }
}
