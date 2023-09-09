using Logistics.Models;

namespace Logistics.Application.Tenant.Commands;

public class SaveEmployeeGeolocationCommand : Request<ResponseResult>
{
    public SaveEmployeeGeolocationCommand(GeolocationData? geolocationData)
    {
        GeolocationData = geolocationData;
    }
    
    public GeolocationData? GeolocationData { get; set; }
}
