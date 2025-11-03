using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DriverApp.Models;

public class ActiveLoad : ObservableRecipient
{
    private Guid? _assignedDispatcherId;

    private string? _assignedDispatcherName;

    private IEnumerable<string>? _assignedTruckDriversName;

    private Guid? _assignedTruckId;

    private string? _assignedTruckNumber;

    private bool _canConfirmDelivery;

    private bool _canConfirmPickUp;

    private decimal _deliveryCost;

    private DateTime? _deliveryDate;

    private Address? _destinationAddress;

    private double? _destinationAddressLat;

    private double? _destinationAddressLong;

    private DateTime? _dispatchedDate;

    private double _distance;

    private Guid? _id;

    private string? _name;

    private long _number;

    private Address? _originAddress;

    private double? _originAddressLat;

    private double? _originAddressLong;

    private DateTime? _pickUpDate;

    private LoadStatus _status;

    public ActiveLoad()
    {
    }

    public ActiveLoad(LoadDto loadDto)
    {
        UpdateFromDto(loadDto);
    }

    public Guid? Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public long Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public Address? OriginAddress
    {
        get => _originAddress;
        set => SetProperty(ref _originAddress, value);
    }

    public double? OriginAddressLat
    {
        get => _originAddressLat;
        set => SetProperty(ref _originAddressLat, value);
    }

    public double? OriginAddressLong
    {
        get => _originAddressLong;
        set => SetProperty(ref _originAddressLong, value);
    }

    public Address? DestinationAddress
    {
        get => _destinationAddress;
        set => SetProperty(ref _destinationAddress, value);
    }

    public double? DestinationAddressLat
    {
        get => _destinationAddressLat;
        set => SetProperty(ref _destinationAddressLat, value);
    }

    public double? DestinationAddressLong
    {
        get => _destinationAddressLong;
        set => SetProperty(ref _destinationAddressLong, value);
    }

    public decimal DeliveryCost
    {
        get => _deliveryCost;
        set => SetProperty(ref _deliveryCost, value);
    }

    public double Distance
    {
        get => _distance;
        set => SetProperty(ref _distance, value);
    }

    public DateTime? DispatchedDate
    {
        get => _dispatchedDate;
        set => SetProperty(ref _dispatchedDate, value);
    }

    public DateTime? PickUpDate
    {
        get => _pickUpDate;
        set => SetProperty(ref _pickUpDate, value);
    }

    public DateTime? DeliveryDate
    {
        get => _deliveryDate;
        set => SetProperty(ref _deliveryDate, value);
    }

    public LoadStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool CanConfirmPickUp
    {
        get => _canConfirmPickUp;
        set => SetProperty(ref _canConfirmPickUp, value);
    }

    public bool CanConfirmDelivery
    {
        get => _canConfirmDelivery;
        set => SetProperty(ref _canConfirmDelivery, value);
    }

    public Guid? AssignedDispatcherId
    {
        get => _assignedDispatcherId;
        set => SetProperty(ref _assignedDispatcherId, value);
    }

    public string? AssignedDispatcherName
    {
        get => _assignedDispatcherName;
        set => SetProperty(ref _assignedDispatcherName, value);
    }

    public Guid? AssignedTruckId
    {
        get => _assignedTruckId;
        set => SetProperty(ref _assignedTruckId, value);
    }

    public string? AssignedTruckNumber
    {
        get => _assignedTruckNumber;
        set => SetProperty(ref _assignedTruckNumber, value);
    }

    public IEnumerable<string>? AssignedTruckDriversName
    {
        get => _assignedTruckDriversName;
        set => SetProperty(ref _assignedTruckDriversName, value);
    }

    public void UpdateFromDto(LoadDto loadDto)
    {
        Id = loadDto.Id;
        Number = loadDto.Number;
        Name = loadDto.Name;
        OriginAddress = loadDto.OriginAddress;
        OriginAddressLong = loadDto.OriginLocation.Longitude;
        OriginAddressLat = loadDto.OriginLocation.Latitude;
        DestinationAddress = loadDto.DestinationAddress;
        DestinationAddressLong = loadDto.DestinationLocation.Longitude;
        DestinationAddressLat = loadDto.DestinationLocation.Latitude;
        DeliveryCost = loadDto.DeliveryCost;
        Distance = loadDto.Distance;
        DispatchedDate = loadDto.DispatchedAt;
        PickUpDate = loadDto.DeliveredAt;
        DeliveryDate = loadDto.DeliveredAt;
        CanConfirmPickUp = loadDto.CanConfirmPickUp;
        CanConfirmDelivery = loadDto.CanConfirmDelivery;
        Status = loadDto.Status;
        AssignedDispatcherId = loadDto.AssignedDispatcherId;
        AssignedDispatcherName = loadDto.AssignedDispatcherName;
        AssignedTruckId = loadDto.AssignedTruckId;
        AssignedTruckNumber = loadDto.AssignedTruckNumber;
        AssignedTruckDriversName = loadDto.AssignedTruckDriversName;
    }
}
