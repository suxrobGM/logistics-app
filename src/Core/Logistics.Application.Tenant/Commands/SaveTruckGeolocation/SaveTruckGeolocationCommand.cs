using Logistics.Models;

namespace Logistics.Application.Tenant.Commands;

public class SaveTruckGeolocationCommand : Request<ResponseResult>
{
    public SaveTruckGeolocationCommand(TruckGeolocationDto? geolocationData)
    {
        GeolocationData = geolocationData;
    }
    
    public TruckGeolocationDto? GeolocationData { get; set; }
}
