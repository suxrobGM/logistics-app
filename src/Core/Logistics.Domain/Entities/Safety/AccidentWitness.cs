using Logistics.Domain.Core;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// Witness information for an accident report
/// </summary>
public class AccidentWitness : Entity, ITenantEntity
{
    public Guid AccidentReportId { get; set; }
    public virtual AccidentReport AccidentReport { get; set; } = null!;

    public required string Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Statement { get; set; }
}
