using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface IStatsApi
{
    Task<ResponseResult<DailyGrossesDto>> GetDailyGrossesAsync(GetDailyGrossesQuery query);
    Task<ResponseResult<MonthlyGrossesDto>> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query);
    Task<ResponseResult<DriverStatsDto>> GetDriverStatsAsync(string userId);
}
