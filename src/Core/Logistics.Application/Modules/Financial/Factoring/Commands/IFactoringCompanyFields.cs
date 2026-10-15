using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

/// <summary>
/// The profile fields shared by the create and update commands, so both inherit one set of rules.
/// </summary>
public interface IFactoringCompanyFields
{
    string Name { get; }
    Address? RemitToAddress { get; }
    string SubmissionEmail { get; }
    string? Phone { get; }
    decimal AdvanceRatePercent { get; }
    decimal FeePercent { get; }
}
