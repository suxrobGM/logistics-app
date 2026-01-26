using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class PayrollReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<PayrollReportQuery, Result<PayrollReportDto>>
{
    public async Task<Result<PayrollReportDto>> Handle(PayrollReportQuery req, CancellationToken ct)
    {
        var payrollsQuery = tenantUow.Repository<PayrollInvoice>().Query();

        if (req.StartDate != default)
        {
            var from = DateTime.SpecifyKind(req.StartDate, DateTimeKind.Utc);
            payrollsQuery = payrollsQuery.Where(p => p.PeriodStart >= from);
        }

        if (req.EndDate != default)
        {
            var to = DateTime.SpecifyKind(req.EndDate, DateTimeKind.Utc);
            payrollsQuery = payrollsQuery.Where(p => p.PeriodEnd <= to);
        }

        var payrolls = payrollsQuery.ToList();

        var totalPayroll = payrolls.Sum(p => p.Total.Amount);
        var totalPaid = payrolls.Sum(p => p.Payments.Sum(pay => pay.Amount.Amount));
        var totalOutstanding = totalPayroll - totalPaid;
        var uniqueEmployees = payrolls.Select(p => p.EmployeeId).Distinct().Count();
        var averagePayrollAmount = payrolls.Any() ? totalPayroll / payrolls.Count : 0;

        var statusBreakdown = CalculateStatusBreakdown(payrolls);
        var payrollTrends = CalculatePayrollTrends(
            payrolls,
            req.StartDate == default ? payrolls.Any() ? payrolls.Min(p => p.PeriodStart) : DateTime.UtcNow.AddMonths(-6) : req.StartDate,
            req.EndDate == default ? payrolls.Any() ? payrolls.Max(p => p.PeriodEnd) : DateTime.UtcNow : req.EndDate
        );
        var topEmployees = GetTopEmployeesByEarnings(payrolls);
        var salaryTypeBreakdown = CalculateSalaryTypeBreakdown(payrolls, totalPayroll);

        var dto = new PayrollReportDto
        {
            TotalPayroll = totalPayroll,
            TotalPaid = totalPaid,
            TotalOutstanding = totalOutstanding,
            TotalPayrolls = payrolls.Count,
            UniqueEmployees = uniqueEmployees,
            AveragePayrollAmount = averagePayrollAmount,
            StatusBreakdown = statusBreakdown,
            PayrollTrends = payrollTrends,
            TopEmployees = topEmployees,
            SalaryTypeBreakdown = salaryTypeBreakdown
        };

        return Result<PayrollReportDto>.Ok(dto);
    }

    private PayrollStatusBreakdownDto CalculateStatusBreakdown(List<PayrollInvoice> payrolls)
    {
        return new PayrollStatusBreakdownDto
        {
            Draft = payrolls.Count(p => p.Status == InvoiceStatus.Draft),
            PendingApproval = payrolls.Count(p => p.Status == InvoiceStatus.PendingApproval),
            Approved = payrolls.Count(p => p.Status == InvoiceStatus.Approved),
            Rejected = payrolls.Count(p => p.Status == InvoiceStatus.Rejected),
            Paid = payrolls.Count(p => p.Status == InvoiceStatus.Paid),
            PartiallyPaid = payrolls.Count(p => p.Status == InvoiceStatus.PartiallyPaid)
        };
    }

    private List<PayrollTrendDto> CalculatePayrollTrends(
        List<PayrollInvoice> payrolls,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<PayrollTrendDto>();
        var currentDate = DateTime.SpecifyKind(new DateTime(startDate.Year, startDate.Month, 1), DateTimeKind.Utc);

        while (currentDate <= endDate)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(currentDate.Year, currentDate.Month, 1), DateTimeKind.Utc);
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var monthPayrolls = payrolls
                .Where(p => p.PeriodStart >= monthStart && p.PeriodStart <= monthEnd)
                .ToList();

            trends.Add(new PayrollTrendDto
            {
                Period = monthStart.ToString("MMM yyyy"),
                TotalAmount = monthPayrolls.Sum(p => p.Total.Amount),
                PaidAmount = monthPayrolls.Sum(p => p.Payments.Sum(pay => pay.Amount.Amount)),
                PayrollCount = monthPayrolls.Count,
                EmployeeCount = monthPayrolls.Select(p => p.EmployeeId).Distinct().Count()
            });

            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }

    private List<TopEmployeePayrollDto> GetTopEmployeesByEarnings(List<PayrollInvoice> payrolls)
    {
        return payrolls
            .GroupBy(p => new { p.EmployeeId, p.Employee })
            .Select(g => new TopEmployeePayrollDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = g.Key.Employee?.GetFullName() ?? "Unknown",
                TotalEarnings = g.Sum(p => p.Total.Amount),
                PayrollCount = g.Count(),
                AverageEarnings = g.Any() ? g.Sum(p => p.Total.Amount) / g.Count() : 0
            })
            .OrderByDescending(e => e.TotalEarnings)
            .Take(10)
            .ToList();
    }

    private List<SalaryTypeBreakdownDto> CalculateSalaryTypeBreakdown(List<PayrollInvoice> payrolls, decimal totalPayroll)
    {
        return payrolls
            .GroupBy(p => p.Employee?.SalaryType ?? SalaryType.None)
            .Select(g => new SalaryTypeBreakdownDto
            {
                SalaryType = GetSalaryTypeLabel(g.Key),
                EmployeeCount = g.Select(p => p.EmployeeId).Distinct().Count(),
                TotalAmount = g.Sum(p => p.Total.Amount),
                Percentage = totalPayroll > 0 ? (double)(g.Sum(p => p.Total.Amount) / totalPayroll) * 100 : 0
            })
            .OrderByDescending(s => s.TotalAmount)
            .ToList();
    }

    private static string GetSalaryTypeLabel(SalaryType type) => type switch
    {
        SalaryType.None => "None",
        SalaryType.Monthly => "Monthly",
        SalaryType.Weekly => "Weekly",
        SalaryType.ShareOfGross => "Share of Gross",
        SalaryType.RatePerDistance => "Rate per Distance",
        SalaryType.Hourly => "Hourly",
        _ => "Unknown"
    };
}
