using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IDriverApi
{
    Task<ResponseResult<DriverActiveLoadsDto>> GetDriverActiveLoadsAsync(string userId);
    Task<ResponseResult<TruckDto>> GetDriverTruckDataAsync(string userId, bool includeLoads = false, bool includeOnlyActiveLoads = false);
    Task<ResponseResult> SetDeviceTokenAsync(SetDeviceToken command);
    Task<ResponseResult> ConfirmLoadStatusAsync(ConfirmLoadStatus command);
    Task<ResponseResult> UpdateLoadProximity(UpdateLoadProximity command);
}
