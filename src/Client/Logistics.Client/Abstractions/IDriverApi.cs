using Logistics.Client.Models;
using Logistics.Shared;

namespace Logistics.Client.Abstractions;

public interface IDriverApi
{
    Task<ResponseResult> SetDeviceTokenAsync(SetDeviceToken command);
    Task<ResponseResult> ConfirmLoadStatusAsync(ConfirmLoadStatus command);
    Task<ResponseResult> UpdateLoadProximity(UpdateLoadProximity command);
}
