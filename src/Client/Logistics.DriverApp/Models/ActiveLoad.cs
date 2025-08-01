using Logistics.Domain.Primitives.Enums;

namespace Logistics.DriverApp.Models;

public class ActiveLoad : ObservableRecipient
{
    public ActiveLoad()
    {
    }
    
    public ActiveLoad(LoadDto loadDto)
    {
        UpdateFromDto(loadDto);
    }

    private Guid? _id;
    public Guid? Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private long _number;
    public long Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }
    
    private string? _name;
    public string? Name 
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private AddressDto? _originAddress;
    public AddressDto? OriginAddress 
    {
        get => _originAddress;
        set => SetProperty(ref _originAddress, value);
    }
    
    private double? _originAddressLat;
    public double? OriginAddressLat 
    {
        get => _originAddressLat;
        set => SetProperty(ref _originAddressLat, value);
    }

    private double? _originAddressLong;
    public double? OriginAddressLong 
    {
        get => _originAddressLong;
        set => SetProperty(ref _originAddressLong, value);
    }

    private AddressDto? _destinationAddress;
    public AddressDto? DestinationAddress 
    {
        get => _destinationAddress;
        set => SetProperty(ref _destinationAddress, value);
    }

    private double? _destinationAddressLat;
    public double? DestinationAddressLat 
    {
        get => _destinationAddressLat;
        set => SetProperty(ref _destinationAddressLat, value);
    }

    private double? _destinationAddressLong;
    public double? DestinationAddressLong 
    {
        get => _destinationAddressLong;
        set => SetProperty(ref _destinationAddressLong, value);
    }

    private decimal _deliveryCost;
    public decimal DeliveryCost 
    {
        get => _deliveryCost;
        set => SetProperty(ref _deliveryCost, value);
    }

    private double _distance;
    public double Distance 
    {
        get => _distance;
        set => SetProperty(ref _distance, value);
    }

    private DateTime _dispatchedDate;
    public DateTime DispatchedDate 
    {
        get => _dispatchedDate;
        set => SetProperty(ref _dispatchedDate, value);
    }

    private DateTime? _pickUpDate;
    public DateTime? PickUpDate 
    {
        get => _pickUpDate;
        set => SetProperty(ref _pickUpDate, value);
    }

    private DateTime? _deliveryDate;
    public DateTime? DeliveryDate 
    {
        get => _deliveryDate;
        set => SetProperty(ref _deliveryDate, value);
    }

    private LoadStatus _status;
    public LoadStatus Status 
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
    
    private bool _canConfirmPickUp;
    public bool CanConfirmPickUp
    {
        get => _canConfirmPickUp;
        set => SetProperty(ref _canConfirmPickUp, value);
    }
    
    private bool _canConfirmDelivery;
    public bool CanConfirmDelivery
    {
        get => _canConfirmDelivery;
        set => SetProperty(ref _canConfirmDelivery, value);
    }

    private Guid? _assignedDispatcherId;
    public Guid? AssignedDispatcherId 
    {
        get => _assignedDispatcherId;
        set => SetProperty(ref _assignedDispatcherId, value);
    }

    private string? _assignedDispatcherName;
    public string? AssignedDispatcherName 
    {
        get => _assignedDispatcherName;
        set => SetProperty(ref _assignedDispatcherName, value);
    }

    private Guid? _assignedTruckId;
    public Guid? AssignedTruckId 
    {
        get => _assignedTruckId;
        set => SetProperty(ref _assignedTruckId, value);
    }

    private string? _assignedTruckNumber;
    public string? AssignedTruckNumber 
    {
        get => _assignedTruckNumber;
        set => SetProperty(ref _assignedTruckNumber, value);
    }

    private IEnumerable<string>? _assignedTruckDriversName;
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
        OriginAddressLat = loadDto.OriginAddressLat;
        OriginAddressLong = loadDto.OriginAddressLong;
        DestinationAddress = loadDto.DestinationAddress;
        DestinationAddressLat = loadDto.DestinationAddressLat;
        DestinationAddressLong = loadDto.DestinationAddressLong;
        DeliveryCost = loadDto.DeliveryCost;
        Distance = loadDto.Distance;
        DispatchedDate = loadDto.DispatchedDate;
        PickUpDate = loadDto.DeliveryDate;
        DeliveryDate = loadDto.DeliveryDate;
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
