namespace Logistics.Application.Abstractions.Dispatch;

public sealed record EligibilityResult(bool IsEligible, IReadOnlyList<EligibilityIssue> Issues)
{
    public static EligibilityResult Ok() => new(true, []);
    public static EligibilityResult Block(params EligibilityIssue[] issues) =>
        new(issues.All(i => i.Severity != EligibilitySeverity.Error), issues);
}

public sealed record EligibilityIssue(
    EligibilityIssueCode Code,
    EligibilitySeverity Severity,
    string Message);

public enum EligibilitySeverity
{
    Warning,
    Error
}

public enum EligibilityIssueCode
{
    TruckNotFound,
    LoadNotFound,
    DriverNotAssigned,
    DriverNotFound,
    NoActiveLicense,
    LicenseExpired,
    LicenseClassInsufficient,
    MissingHazmatEndorsement,
    MissingAdrEndorsement,
    MissingAdrClass1Endorsement,
    MissingAdrClass7Endorsement,
    AdrCertExpired,
    TruckNotAdrCertified,
    TruckClassNotAllowed,
    HazmatPlacardingRequired,
    MedicalCertExpired,
    MedicalCertExpiringSoon
}
