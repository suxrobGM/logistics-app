using System.ComponentModel.DataAnnotations;

namespace Logistics.Application.Tenant.Commands;

public class CreateTruckCommand : Request<ResponseResult>
{
    [Required]
    public int? TruckNumber { get; set; }
    
    [Required]
    public string? DriverId { get; set; }
}
