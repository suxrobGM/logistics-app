using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class UpdateContactSubmissionCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public ContactSubmissionStatus Status { get; set; }
    public string? Notes { get; set; }
}
