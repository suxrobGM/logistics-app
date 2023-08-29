using System.ComponentModel.DataAnnotations;

namespace Logistics.Application.Tenant.Commands;

public sealed class CreateTruckCommand : RequestBase<ResponseResult>
{
    [Required]
    public int? TruckNumber { get; set; }
    
    [Required]
    public string? DriverId { get; set; }
}
