using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IStatsApi
{
    Task<ResponseResult<DailyGrossesDto>> GetDailyGrossesAsync(GetDailyGrossesQuery query);
    Task<ResponseResult<MonthlyGrossesDto>> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query);
    Task<ResponseResult<DriverStatsDto>> GetDriverStatsAsync(string userId);
}
