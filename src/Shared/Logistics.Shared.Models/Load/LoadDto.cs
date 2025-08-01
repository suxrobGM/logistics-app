﻿using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

public class LoadDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string? Name { get; set; }
    public required Address OriginAddress { get; set; }
    public double? OriginAddressLat { get; set; }
    public double? OriginAddressLong { get; set; }
    public required Address DestinationAddress { get; set; }
    public double? DestinationAddressLat { get; set; }
    public double? DestinationAddressLong { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public DateTime DispatchedDate { get; set; }
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public bool CanConfirmPickUp { get; set; }
    public bool CanConfirmDelivery { get; set; }
    public LoadStatus Status { get; set; }
    public LoadType Type { get; set; }
    public Guid? AssignedDispatcherId { get; set; }
    public string? AssignedDispatcherName { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public string? AssignedTruckNumber { get; set; }
    public Address? CurrentLocation { get; set; }
    public CustomerDto? Customer { get; set; }
    
    public Guid? TripId { get; set; }
    public long? TripNumber { get; set; }
    public string? TripName { get; set; }
    
    public IEnumerable<string>? AssignedTruckDriversName { get; set; }
    public IEnumerable<InvoiceDto> Invoices { get; set; } = [];
}
