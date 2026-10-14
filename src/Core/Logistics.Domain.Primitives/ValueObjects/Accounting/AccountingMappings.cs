namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Which accounting-provider item or account each TMS category posts to. Anything unmapped falls
/// back to the connection's default item or account.
/// </summary>
public record AccountingMappings
{
    /// <summary>Invoice line type (base rate, fuel surcharge, detention...) to a provider sales item.</summary>
    public List<LineItemTypeMapping> Items { get; set; } = [];

    /// <summary>Company expense category to a provider expense account.</summary>
    public List<CompanyExpenseCategoryMapping> CompanyExpenseAccounts { get; set; } = [];

    /// <summary>Truck expense category (fuel, tolls, tires...) to a provider expense account.</summary>
    public List<TruckExpenseCategoryMapping> TruckExpenseAccounts { get; set; } = [];
}
