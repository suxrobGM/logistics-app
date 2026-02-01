using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Services;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds payroll invoices using PayrollService.
/// </summary>
internal class PayrollSeeder(ILogger<PayrollSeeder> logger) : SeederBase(logger)
{
    private readonly DateTime startDate = DateTime.UtcNow.AddMonths(-2);
    private readonly DateTime endDate = DateTime.UtcNow.AddDays(-1);

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

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var payrollService = context.ServiceProvider.GetRequiredService<PayrollService>();

        await payrollService.GeneratePayrolls(employees, startDate, endDate);

        LogCompleted();
    }
}
