using Logistics.Client.Models;
using Logistics.Shared;

namespace Logistics.Client.Abstractions;

public interface IDriverApi
{
    Task<Result> SetDeviceTokenAsync(SetDeviceToken command);
    Task<Result> ConfirmLoadStatusAsync(ConfirmLoadStatus command);
    Task<Result> UpdateLoadProximity(UpdateLoadProximity command);
}
