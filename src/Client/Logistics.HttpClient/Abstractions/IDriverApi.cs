using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IDriverApi
{
    Task<Result> SetDeviceTokenAsync(SetDeviceToken command);
    Task<Result> ConfirmLoadStatusAsync(ConfirmLoadStatus command);
    Task<Result> UpdateLoadProximity(UpdateLoadProximityCommand command);
}
