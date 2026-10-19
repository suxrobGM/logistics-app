using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds paid payroll history through the app's <see cref="IPayrollService"/>, on the same monthly and
/// weekly cadences the payroll generation job uses.
/// </summary>
internal class PayrollSeeder(ILogger<PayrollSeeder> logger) : SeederBase(logger)
{
    private static readonly SalaryType[] MonthlySalaryTypes = [SalaryType.Monthly, SalaryType.ShareOfGross];
    private static readonly SalaryType[] WeeklySalaryTypes = [SalaryType.Weekly, SalaryType.PerLoad, SalaryType.PerStop];

    private readonly DateTime _startDate = DateTime.UtcNow.AddMonths(-2);
    private readonly DateTime _endDate = DateTime.UtcNow.AddDays(-1);

    public override string Name => nameof(PayrollSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 170;
    public override IReadOnlyList<string> DependsOn => [nameof(EmployeeSeeder), nameof(LoadSeeder), nameof(TripSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<PayrollInvoice>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees;
        if (employees is null)
        {
            LogUpstreamSkipped();
            return;
        }

        var payrollService = context.ServiceProvider.GetRequiredService<IPayrollService>();

        await SeedPeriodsAsync(context, payrollService, employees, MonthlySalaryTypes,
            DateRangeGenerator.GenerateMonthlyRanges(_startDate, _endDate), cancellationToken);
        await SeedPeriodsAsync(context, payrollService, employees, WeeklySalaryTypes,
            DateRangeGenerator.GenerateWeeklyRanges(_startDate, _endDate), cancellationToken);

        LogCompleted();
    }

    /// <summary>
    /// Saves after every period: a settlement claims cash advances and fuel charges in memory, and the
    /// next period's query sees the claim only once it is saved.
    /// </summary>
    private static async Task SeedPeriodsAsync(
        SeederContext context,
        IPayrollService payrollService,
        CompanyEmployees employees,
        SalaryType[] salaryTypes,
        List<(DateTime StartDate, DateTime EndDate)> periods,
        CancellationToken ct)
    {
        var payees = employees.AllEmployees.Where(e => salaryTypes.Contains(e.SalaryType)).ToList();
        var payrollRepository = context.TenantUnitOfWork.Repository<PayrollInvoice>();

        foreach (var (startDate, endDate) in periods)
        {
            foreach (var employee in payees)
            {
                var payroll = await payrollService.CreatePayrollInvoiceAsync(
                    employee, DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate), ct);
                payroll.Status = InvoiceStatus.Paid;
                await payrollRepository.AddAsync(payroll, ct);
            }

            await context.TenantUnitOfWork.SaveChangesAsync(ct);
        }
    }
}
