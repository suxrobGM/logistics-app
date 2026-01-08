using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IStatsApi
{
    Task<DailyGrossesDto?> GetDailyGrossesAsync(GetDailyGrossesQuery query);
    Task<MonthlyGrossesDto?> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query);
    Task<DriverStatsDto?> GetDriverStatsAsync(Guid userId);
}
