using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

public class UpdateFactoringCompanyCommand : ICommand<Result>, IHaveId, IFactoringCompanyFields
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Address? RemitToAddress { get; set; }
    public string SubmissionEmail { get; set; } = null!;
    public string? Phone { get; set; }
    public FactoringRecourseType RecourseType { get; set; }
    public decimal AdvanceRatePercent { get; set; }
    public decimal FeePercent { get; set; }
    public bool IsDefault { get; set; }
}
