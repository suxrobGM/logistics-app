using Logistics.Domain.Persistence;

namespace Logistics.DbMigrator.Data;

internal class CreateSqlFunctionsWorker(
    ILogger<CreateSqlFunctionsWorker> logger,
    IServiceScopeFactory scopeFactory)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();

            logger.LogInformation("Creating Stored Procedures");
            await CreateSqlFunction("CreateCompanyStats.psql", tenantUow);
            await CreateSqlFunction("CreateTrucksStats.psql", tenantUow);

            logger.LogInformation("Stored Procedures Created");
        }
        catch (Exception ex)
        {
            logger.LogError("Thrown exception in PopulateData.ExecuteAsync(): {Exception}", ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task CreateSqlFunction(string fileName, ITenantUnitOfWork tenantUow)
    {
        var sql = await File.ReadAllTextAsync($"SqlFunctions/{fileName}");
        await tenantUow.ExecuteRawSql(sql);
    }
}
