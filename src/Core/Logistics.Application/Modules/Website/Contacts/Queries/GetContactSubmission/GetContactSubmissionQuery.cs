using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.Contacts.Queries;

public sealed class GetContactSubmissionQuery : IQuery<Result<ContactSubmissionDto>>
{
    public Guid Id { get; set; }
}
