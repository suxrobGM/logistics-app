using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface IStatsApi
{
    Task<Result<DailyGrossesDto>> GetDailyGrossesAsync(GetDailyGrossesQuery query);
    Task<Result<MonthlyGrossesDto>> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query);
    Task<Result<DriverStatsDto>> GetDriverStatsAsync(string userId);
}
