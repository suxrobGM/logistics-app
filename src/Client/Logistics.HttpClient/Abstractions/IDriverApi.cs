using Logistics.HttpClient.Models;
using Logistics.Shared;

namespace Logistics.HttpClient.Abstractions;

public interface IDriverApi
{
    Task<Result> SetDeviceTokenAsync(SetDeviceToken command);
    Task<Result> ConfirmLoadStatusAsync(ConfirmLoadStatus command);
    Task<Result> UpdateLoadProximity(UpdateLoadProximity command);
}
