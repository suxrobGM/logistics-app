using Logistics.Domain.Persistence;

namespace Logistics.DbMigrator.Data;

internal class CreateSqlFunctionsWorker : IHostedService
{
    private readonly ILogger<CreateSqlFunctionsWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateSqlFunctionsWorker(ILogger<CreateSqlFunctionsWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();

            _logger.LogInformation("Creating Stored Procedures");
            await CreateSqlFunction("CreateCompanyStats.psql", tenantUow);
            await CreateSqlFunction("CreateTrucksStats.psql", tenantUow);

            _logger.LogInformation("Stored Procedures Created");
        }
        catch (Exception ex)
        {
            _logger.LogError("Thrown exception in PopulateData.ExecuteAsync(): {Exception}", ex);
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
