using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

/// <summary>
/// Record the factor's answer on a schedule: which invoices it bought, with the advance and fee it took,
/// and which it turned down. Invoices left out stay submitted.
/// </summary>
public class RecordFactoringFundingCommand : ICommand<Result>
{
    public Guid BatchId { get; set; }
    public DateTime FundedAt { get; set; }
    public List<FactoringFundingLine> Invoices { get; set; } = [];
}

public record FactoringFundingLine
{
    public Guid ItemId { get; set; }
    public bool Rejected { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public string? RejectionReason { get; set; }
}
