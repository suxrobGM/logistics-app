using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetContactSubmissionQuery : IQuery<Result<ContactSubmissionDto>>
{
    public Guid Id { get; set; }
}
