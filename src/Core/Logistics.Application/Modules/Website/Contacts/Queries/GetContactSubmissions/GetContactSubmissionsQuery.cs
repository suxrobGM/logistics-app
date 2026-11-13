using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.Contacts.Queries;

public sealed class GetContactSubmissionsQuery : SearchableQuery, IQuery<PagedResult<ContactSubmissionDto>>
{
}
