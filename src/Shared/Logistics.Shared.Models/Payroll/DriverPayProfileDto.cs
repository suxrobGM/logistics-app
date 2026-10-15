namespace Logistics.Shared.Models;

/// <summary>
/// How a driver's settlement is built beyond the salary type: what share-of-gross pay excludes,
/// whether fuel card purchases are deducted, the recurring deductions and the escrow held.
/// </summary>
public record DriverPayProfileDto
{
    public Guid EmployeeId { get; set; }
    public bool PayExcludesFuelSurcharge { get; set; }
    public bool PayExcludesAccessorials { get; set; }
    public bool DeductFuelCardCharges { get; set; }
    public List<RecurringDeductionDto> RecurringDeductions { get; set; } = [];
    public decimal EscrowBalance { get; set; }

    /// <summary>Approved cash advances not yet deducted from a settlement.</summary>
    public decimal OutstandingAdvances { get; set; }
}
