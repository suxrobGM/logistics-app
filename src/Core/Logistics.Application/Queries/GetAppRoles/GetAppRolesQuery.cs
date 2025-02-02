using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetAppRolesQuery : SearchableQuery, IRequest<PagedResult<AppRoleDto>>
{
}
