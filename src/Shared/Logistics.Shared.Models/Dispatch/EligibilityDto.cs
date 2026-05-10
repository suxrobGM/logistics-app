namespace Logistics.Shared.Models;

public class EligibilityResultDto
{
    public bool IsEligible { get; set; }
    public IList<EligibilityIssueDto> Issues { get; set; } = [];
}

public class EligibilityIssueDto
{
    public string Code { get; set; } = null!;
    public string Severity { get; set; } = null!;
    public string Message { get; set; } = null!;
}
