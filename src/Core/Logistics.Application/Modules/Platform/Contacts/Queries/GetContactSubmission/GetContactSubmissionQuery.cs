using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Contacts.Queries;

public sealed class GetContactSubmissionQuery : IQuery<Result<ContactSubmissionDto>>
{
    public Guid Id { get; set; }
}
