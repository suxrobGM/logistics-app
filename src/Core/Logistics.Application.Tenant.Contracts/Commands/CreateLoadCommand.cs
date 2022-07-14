﻿namespace Logistics.Application.Contracts.Commands;

public sealed class CreateLoadCommand : RequestBase<DataResult>
{
    public string? Name { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public decimal DeliveryCost { get; set; }
    public double TotalTripMiles { get; set; }
    public string? AssignedDispatcherId { get; set; }
    public string? AssignedTruckId { get; set; }
}