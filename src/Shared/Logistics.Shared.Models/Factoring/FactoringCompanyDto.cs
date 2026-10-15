using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public record FactoringCompanyDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Address? RemitToAddress { get; set; }
    public required string SubmissionEmail { get; set; }
    public string? Phone { get; set; }
    public FactoringRecourseType RecourseType { get; set; }
    public decimal AdvanceRatePercent { get; set; }
    public decimal FeePercent { get; set; }

    /// <summary>
    /// Whatever the advance and the fee leave over; held until the customer pays the factor.
    /// </summary>
    public decimal ReservePercent { get; set; }

    public bool IsDefault { get; set; }
}
