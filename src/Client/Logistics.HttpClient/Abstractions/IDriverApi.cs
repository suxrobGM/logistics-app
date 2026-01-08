using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IDriverApi
{
    Task<bool> SetDeviceTokenAsync(SetDeviceToken command);
    Task<bool> ConfirmLoadStatusAsync(ConfirmLoadStatus command);
    Task<bool> UpdateLoadProximity(UpdateLoadProximityCommand command);
}
