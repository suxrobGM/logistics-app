namespace Logistics.Application.Contracts.Commands;

public sealed class CreateLoadCommand : RequestBase<DataResult>
{
    public string? Name { get; set; }
    
    [Required]
    public string? SourceAddress { get; set; }
    
    [Required]
    public string? DestinationAddress { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    
    [Required]
    public string? AssignedDispatcherId { get; set; }
    
    [Required]
    public string? AssignedDriverId { get; set; }
}
