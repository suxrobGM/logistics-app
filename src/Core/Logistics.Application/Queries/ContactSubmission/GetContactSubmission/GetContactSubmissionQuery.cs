using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetContactSubmissionQuery : IAppRequest<Result<ContactSubmissionDto>>
{
    public Guid Id { get; set; }
}
