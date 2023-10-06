using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services;
using Logistics.Models;

namespace Logistics.DriverApp.ViewModels;

public class LoadPageViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IApiClient _apiClient;
    private readonly IMapsService _mapsService;
    private string? _lastLoadId;

    public LoadPageViewModel(IApiClient apiClient, IMapsService mapsService)
    {
        _apiClient = apiClient;
        _mapsService = mapsService;
        ConfirmPickUpCommand = new AsyncRelayCommand(() => ConfirmLoadStatusAsync(LoadStatusDto.PickedUp));
        ConfirmDeliveryCommand = new AsyncRelayCommand(() => ConfirmLoadStatusAsync(LoadStatusDto.Delivered));
    }

    
    #region Commands

    public IAsyncRelayCommand ConfirmPickUpCommand { get; }
    public IAsyncRelayCommand ConfirmDeliveryCommand { get; }

    #endregion

    
    #region Bindable properties

    private ActiveLoad? _load;
    public ActiveLoad? Load
    {
        get => _load;
        set => SetProperty(ref _load, value);
    }

    private string? _embedMapHtml;
    public string? EmbedMapHtml
    {
        get => _embedMapHtml;
        set => SetProperty(ref _embedMapHtml, value);
    }

    #endregion

    
    protected override async Task OnAppearingAsync()
    {
        await FetchLoadAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query["load"] is ActiveLoad load)
        {
            Load = load;
            EmbedMapHtml = GetEmbedMapHtml(Load);
            _lastLoadId = load.Id;
            return;
        }

        var loadId = query["loadId"] as string;
        
        if (!string.IsNullOrEmpty(loadId) && loadId != _lastLoadId)
        {
            _lastLoadId = loadId;
        }
    }

    private async Task FetchLoadAsync()
    {
        if (string.IsNullOrEmpty(_lastLoadId) || 
            _lastLoadId == Load?.Id)
        {
            return;
        }
        
        IsLoading = true;
        var result = await _apiClient.GetLoadAsync(_lastLoadId);
        
        if (!result.IsSuccess)
        {
            await PopupHelpers.ShowErrorAsync("Failed to fetch the load data, try later");
            IsLoading = false;
            return;
        }

        Load = new ActiveLoad(result.Value!);
        EmbedMapHtml = GetEmbedMapHtml(Load);
        IsLoading = false;
    }

    private async Task ConfirmLoadStatusAsync(LoadStatusDto status)
    {
        if (Load is null)
            return;
        
        IsLoading = true;
        ResponseResult? result = default;

        switch (status)
        {
            case LoadStatusDto.PickedUp:
                Load.Status = LoadStatusDto.PickedUp;
                Load.CanConfirmPickUp = false;
                result = await _apiClient.ConfirmLoadStatusAsync(new ConfirmLoadStatus
                {
                    LoadId = _lastLoadId,
                    LoadStatus = LoadStatusDto.PickedUp
                });
                break;
            case LoadStatusDto.Delivered:
                Load.Status = LoadStatusDto.Delivered;
                Load.CanConfirmDelivery = false;
                result = await _apiClient.ConfirmLoadStatusAsync(new ConfirmLoadStatus
                {
                    LoadId = _lastLoadId,
                    LoadStatus = LoadStatusDto.Delivered
                });
                break;
        }

        if (result is { IsSuccess: false })
        {
            await PopupHelpers.ShowErrorAsync($"Could not confirm the load status, error: {result.Error}");
            IsLoading = false;
            return;
        }

        Load = default;
        await FetchLoadAsync();
        IsLoading = false;
    }

    private string GetEmbedMapHtml(ActiveLoad load)
    {
        var originAddress = $"{load.OriginAddressLat},{load.OriginAddressLong}";
        var destinationAddress = $"{load.DestinationAddressLat},{load.DestinationAddressLong}";
        var embedMapHtml = _mapsService.GetDirectionsMapHtml(originAddress, destinationAddress);
        return embedMapHtml;
    }
}
