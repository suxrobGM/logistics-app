using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IDriverApi
{
    Task<ResponseResult<DriverActiveLoadsDto>> GetDriverActiveLoadsAsync(string userId);
    Task<ResponseResult<TruckDto>> GetDriverTruckAsync(string userId, bool includeLoads = false, bool includeOnlyActiveLoads = false);
    Task<ResponseResult> SetDeviceTokenAsync(string userId, string token);
}
