using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Safety;

namespace Logistics.Domain.Entities.Safety;

/// <summary>
/// A defect found during a DVIR inspection
/// </summary>
public class DvirDefect : Entity, ITenantEntity
{
    public Guid DvirReportId { get; set; }
    public virtual DvirReport DvirReport { get; set; } = null!;

    /// <summary>
    /// The inspection category where the defect was found
    /// </summary>
    public required DvirInspectionCategory Category { get; set; }

    /// <summary>
    /// Description of the defect
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Severity of the defect
    /// </summary>
    public required DefectSeverity Severity { get; set; }

    /// <summary>
    /// Whether this defect has been corrected
    /// </summary>
    public bool IsCorrected { get; set; }

    /// <summary>
    /// Notes about the correction made
    /// </summary>
    public string? CorrectionNotes { get; set; }

    /// <summary>
    /// When the defect was corrected
    /// </summary>
    public DateTime? CorrectedAt { get; set; }

    /// <summary>
    /// Who corrected the defect
    /// </summary>
    public Guid? CorrectedById { get; set; }
    public virtual Employee? CorrectedBy { get; set; }
}
