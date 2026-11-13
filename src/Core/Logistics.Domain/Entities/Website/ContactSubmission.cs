using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

public class ContactSubmission : Entity, IMasterEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public ContactSubject Subject { get; set; }
    public required string Message { get; set; }
    public string? Notes { get; set; }
    public ContactSubmissionStatus Status { get; set; } = ContactSubmissionStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
