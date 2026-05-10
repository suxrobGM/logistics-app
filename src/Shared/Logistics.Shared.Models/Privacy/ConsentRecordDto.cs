using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record ConsentRecordDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }
    public DateTime Timestamp { get; set; }
}
