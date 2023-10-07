using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Commands;

public class SetTruckGeolocationCommand : Request<ResponseResult>
{
    public SetTruckGeolocationCommand(TruckGeolocationDto geolocationData)
    {
        GeolocationData = geolocationData;
    }
    
    public TruckGeolocationDto GeolocationData { get; set; }
}
