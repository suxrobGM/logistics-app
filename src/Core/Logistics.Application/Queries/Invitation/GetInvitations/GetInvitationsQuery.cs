using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetInvitationsQuery : SearchableQuery, IAppRequest<PagedResult<InvitationDto>>
{
    public InvitationStatus? Status { get; set; }
    public InvitationType? Type { get; set; }
}
