using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

public class CreateFactoringCompanyCommand : ICommand<Result<FactoringCompanyDto>>, IFactoringCompanyFields
{
    public string Name { get; set; } = null!;
    public Address? RemitToAddress { get; set; }
    public string SubmissionEmail { get; set; } = null!;
    public string? Phone { get; set; }
    public decimal AdvanceRatePercent { get; set; }
    public decimal FeePercent { get; set; }
}
