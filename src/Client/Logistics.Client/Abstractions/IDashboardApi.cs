using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IDashboardApi
{
    Task<ResponseResult<DriverDashboardDto>> GetDriverDashboardDataAsync(string userId);
}
