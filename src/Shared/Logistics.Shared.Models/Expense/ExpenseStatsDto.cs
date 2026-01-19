namespace Logistics.Shared.Models;

public record ExpenseStatsDto
{
    public decimal TotalAmount { get; init; }
    public int TotalCount { get; init; }
    public decimal PendingAmount { get; init; }
    public int PendingCount { get; init; }
    public decimal ApprovedAmount { get; init; }
    public int ApprovedCount { get; init; }
    public decimal PaidAmount { get; init; }
    public int PaidCount { get; init; }

    // By type
    public List<ExpenseTypeStatDto> ByType { get; init; } = [];

    // By category (company expenses)
    public List<ExpenseCategoryStatDto> ByCompanyCategory { get; init; } = [];

    // By category (truck expenses)
    public List<ExpenseCategoryStatDto> ByTruckCategory { get; init; } = [];

    // Monthly trend
    public List<ExpenseMonthlyStatDto> MonthlyTrend { get; init; } = [];

    // Top trucks by expense
    public List<TruckExpenseStatDto> TopTrucks { get; init; } = [];
}

public record ExpenseTypeStatDto
{
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int Count { get; init; }
}

public record ExpenseCategoryStatDto
{
    public string Category { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int Count { get; init; }
}

public record ExpenseMonthlyStatDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
    public int Count { get; init; }
}

public record TruckExpenseStatDto
{
    public Guid TruckId { get; init; }
    public string TruckNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public int ExpenseCount { get; init; }
}
