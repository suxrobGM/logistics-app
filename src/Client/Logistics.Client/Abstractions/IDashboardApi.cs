using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IDashboardApi
{
    Task<ResponseResult<DailyGrossesDto>> GetDailyGrossesAsync(GetDailyGrossesQuery query);
    Task<ResponseResult<MonthlyGrossesDto>> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query);
}
