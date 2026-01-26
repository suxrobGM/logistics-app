namespace Logistics.Shared.Models;

/// <summary>
/// Payroll report summary with aggregate metrics and trends.
/// </summary>
public record PayrollReportDto
{
    /// <summary>
    /// Total payroll amount for the period.
    /// </summary>
    public decimal TotalPayroll { get; set; }

    /// <summary>
    /// Total amount paid to employees.
    /// </summary>
    public decimal TotalPaid { get; set; }

    /// <summary>
    /// Total outstanding payroll amount.
    /// </summary>
    public decimal TotalOutstanding { get; set; }

    /// <summary>
    /// Number of payroll invoices in the period.
    /// </summary>
    public int TotalPayrolls { get; set; }

    /// <summary>
    /// Number of employees with payroll in the period.
    /// </summary>
    public int UniqueEmployees { get; set; }

    /// <summary>
    /// Average payroll amount per invoice.
    /// </summary>
    public decimal AveragePayrollAmount { get; set; }

    /// <summary>
    /// Breakdown by status.
    /// </summary>
    public PayrollStatusBreakdownDto StatusBreakdown { get; set; } = new();

    /// <summary>
    /// Monthly payroll trends.
    /// </summary>
    public List<PayrollTrendDto> PayrollTrends { get; set; } = [];

    /// <summary>
    /// Top employees by total earnings.
    /// </summary>
    public List<TopEmployeePayrollDto> TopEmployees { get; set; } = [];

    /// <summary>
    /// Breakdown by salary type.
    /// </summary>
    public List<SalaryTypeBreakdownDto> SalaryTypeBreakdown { get; set; } = [];
}

/// <summary>
/// Payroll status breakdown.
/// </summary>
public class PayrollStatusBreakdownDto
{
    public int Draft { get; set; }
    public int PendingApproval { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Paid { get; set; }
    public int PartiallyPaid { get; set; }
}

/// <summary>
/// Monthly payroll trend data.
/// </summary>
public class PayrollTrendDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public int PayrollCount { get; set; }
    public int EmployeeCount { get; set; }
}

/// <summary>
/// Top employee by payroll earnings.
/// </summary>
public class TopEmployeePayrollDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal TotalEarnings { get; set; }
    public int PayrollCount { get; set; }
    public decimal AverageEarnings { get; set; }
}

/// <summary>
/// Salary type breakdown.
/// </summary>
public class SalaryTypeBreakdownDto
{
    public string SalaryType { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal TotalAmount { get; set; }
    public double Percentage { get; set; }
}
