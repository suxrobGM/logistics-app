using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class CreateContactSubmissionCommand : IAppRequest<Result>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ContactSubject Subject { get; set; }
    public string Message { get; set; } = string.Empty;
}
