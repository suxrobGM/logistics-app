using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public class GetAppRolesQuery : SearchableQuery, IRequest<PagedResponseResult<AppRoleDto>>
{
}
