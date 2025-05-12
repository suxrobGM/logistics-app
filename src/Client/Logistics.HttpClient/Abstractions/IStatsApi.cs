using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IStatsApi
{
    Task<Result<DailyGrossesDto>> GetDailyGrossesAsync(GetDailyGrossesQuery query);
    Task<Result<MonthlyGrossesDto>> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query);
    Task<Result<DriverStatsDto>> GetDriverStatsAsync(Guid userId);
}
