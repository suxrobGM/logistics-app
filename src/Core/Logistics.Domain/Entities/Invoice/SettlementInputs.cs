namespace Logistics.Domain.Entities;

/// <summary>
/// Everything a driver settlement is built from besides the period itself.
/// </summary>
/// <param name="GrossPay">Pay earned in the period under the employee's salary type.</param>
/// <param name="EscrowBalance">Escrow held for the driver before this settlement.</param>
/// <param name="CashAdvances">Approved advances not yet deducted.</param>
/// <param name="FuelCharges">Fuel card purchases to deduct.</param>
public sealed record SettlementInputs(
    decimal GrossPay,
    string GrossPayDescription,
    IReadOnlyList<RecurringDeduction> RecurringDeductions,
    decimal EscrowBalance,
    IReadOnlyList<CashAdvance> CashAdvances,
    IReadOnlyList<FuelCardTransaction> FuelCharges)
{
    public static SettlementInputs GrossOnly(decimal grossPay, string description) =>
        new(grossPay, description, [], 0, [], []);
}
