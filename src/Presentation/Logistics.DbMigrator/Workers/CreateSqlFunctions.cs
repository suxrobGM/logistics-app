using Logistics.Domain.Persistence;

namespace Logistics.DbMigrator.Data;

internal class CreateSqlFunctions
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IConfiguration _configuration;
    public CreateSqlFunctions(ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _tenantUow = serviceProvider.GetRequiredService<ITenantUnityOfWork>();
        _masterUow = serviceProvider.GetRequiredService<IMasterUnityOfWork>();
        _configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    public async Task ExecuteAsync()
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
