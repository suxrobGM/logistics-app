using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public record CreateDriverCertificationCommand : IAppRequest<Result<DriverCertificationDto>>
{
    public required Guid EmployeeId { get; set; }
    public required CertificationType Type { get; set; }
    public required string CertificationNumber { get; set; }
    public string? IssuingAuthority { get; set; }
    public string? IssuingState { get; set; }
    public required DateTime IssueDate { get; set; }
    public required DateTime ExpirationDate { get; set; }
    public CdlClass? CdlClass { get; set; }
    public string? Endorsements { get; set; }
    public string? Restrictions { get; set; }
    public string? Notes { get; set; }
}
