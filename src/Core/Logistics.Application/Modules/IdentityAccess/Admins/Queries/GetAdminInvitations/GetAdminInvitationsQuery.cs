using Logistics.Application.Abstractions.Common;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Admins.Queries;

/// <summary>
/// Lists pending app-level (Admin) invitations.
/// </summary>
public sealed class GetAdminInvitationsQuery : SearchableQuery, IQuery<PagedResult<InvitationDto>>
{
}
