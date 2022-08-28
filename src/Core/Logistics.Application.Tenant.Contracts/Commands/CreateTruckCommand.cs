namespace Logistics.Application.Contracts.Commands;

public sealed class CreateTruckCommand : RequestBase<DataResult>
{
    [Required]
    public int? TruckNumber { get; set; }
    
    [Required]
    public string? DriverId { get; set; }
}
