using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetContactSubmissionsQuery : SearchableQuery, IAppRequest<PagedResult<ContactSubmissionDto>>
{
}
