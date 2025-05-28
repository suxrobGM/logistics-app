using Logistics.Domain.Persistence;

namespace Logistics.DbMigrator.Data;

internal class CreateSqlFunctionsWorker :IHostedService
{
    private readonly ILogger _logger;
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IConfiguration _configuration;
    public CreateSqlFunctionsWorker(ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var populate = _configuration.GetValue<bool>("PopulateFakeData");

            if (!populate)
            {
                return;
            }

            _logger.LogInformation("Creating Stored Procedures");
            await CreateCompanyStatsFunction();
            await CreateTrucksStatsFunction();

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

    private async Task CreateCompanyStatsFunction()
    {
        var sql = await File.ReadAllTextAsync("SqlFunctions/CreateCompanyStats.psql");
        await _tenantUow.ExecuteRawSql(sql);
    }
    private async Task CreateTrucksStatsFunction()
    {
        var sql = await File.ReadAllTextAsync("SqlFunctions/CreateTrucksStats.psql");
        await _tenantUow.ExecuteRawSql(sql);
    }


}
