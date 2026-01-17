using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetInvitationByIdQuery : IAppRequest<Result<InvitationDto>>
{
    public Guid Id { get; set; }
}
