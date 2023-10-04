using CommunityToolkit.Mvvm.Messaging;
using Logistics.DriverApp.Messages;
using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services;
using Logistics.Models;

namespace Logistics.DriverApp.ViewModels;

public class LoadPageViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IApiClient _apiClient;
    private readonly IMapsService _mapsService;
    private string? _lastLoadId;
    private bool _isOpened;

    public LoadPageViewModel(IApiClient apiClient, IMapsService mapsService)
    {
        _apiClient = apiClient;
        _mapsService = mapsService;
        ConfirmPickUpCommand = new AsyncRelayCommand(() => ConfirmLoadStatusAsync(LoadStatusDto.PickedUp));
        ConfirmDeliveryCommand = new AsyncRelayCommand(() => ConfirmLoadStatusAsync(LoadStatusDto.Delivered));
        
        Messenger.Register<ActiveLoadsChangedMessage>(this, (_, m) =>
        {
            if (!_isOpened || string.IsNullOrEmpty(_lastLoadId))
            {
                return;
            }
            
            var load = m.Value.FirstOrDefault(i => i.Id == _lastLoadId);

            if (load is not null)
            {
                ActiveLoad = new ActiveLoad(load, GetEmbedMapHtml(load));
            }
        });
    }

    #region Commands

    public IAsyncRelayCommand ConfirmPickUpCommand { get; }
    public IAsyncRelayCommand ConfirmDeliveryCommand { get; }

    #endregion

    
    #region Bindable properties

    private ActiveLoad? _activeLoad;
    public ActiveLoad? ActiveLoad
    {
        get => _activeLoad;
        set => SetProperty(ref _activeLoad, value);
    }

    #endregion

    
    protected override async Task OnAppearingAsync()
    {
        _isOpened = true;
        await FetchLoadAsync();
    }

    protected override Task OnDisappearingAsync()
    {
        _isOpened = false;
        return base.OnDisappearingAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query["load"] is LoadDto load)
        {
            var embedMapHtml = GetEmbedMapHtml(load);
            ActiveLoad = new ActiveLoad(load, embedMapHtml);
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
            _lastLoadId == ActiveLoad?.LoadData.Id)
        {
            return;
        }
        
        IsLoading = true;
        var result = await _apiClient.GetLoadAsync(_lastLoadId);
        
        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync("Failed to fetch the load data, try later");
            IsLoading = false;
            return;
        }

        var loadDto = result.Value!;
        var embedMapHtml = GetEmbedMapHtml(loadDto);
        ActiveLoad = new ActiveLoad(loadDto, embedMapHtml);
        IsLoading = false;
    }

    private async Task ConfirmLoadStatusAsync(LoadStatusDto status)
    {
        IsLoading = true;

        var result = status switch
        {
            LoadStatusDto.PickedUp => await _apiClient.ConfirmLoadStatusAsync(new ConfirmLoadStatus
            {
                LoadId = _lastLoadId, 
                LoadStatus = LoadStatusDto.PickedUp
            }),
            LoadStatusDto.Delivered => await _apiClient.ConfirmLoadStatusAsync(new ConfirmLoadStatus
            {
                LoadId = _lastLoadId, 
                LoadStatus = LoadStatusDto.Delivered
            }),
            _ => default
        };

        if (result is { Success: false })
        {
            await PopupHelpers.ShowErrorAsync($"Could not confirm the load status, error: {result.Error}");
            IsLoading = false;
            return;
        }

        ActiveLoad = null;
        await FetchLoadAsync();
        IsLoading = false;
    }

    private string GetEmbedMapHtml(LoadDto load)
    {
        var originAddress = $"{load.OriginAddressLat},{load.OriginAddressLong}";
        var destinationAddress = $"{load.DestinationAddressLat},{load.DestinationAddressLong}";
        var embedMapHtml = _mapsService.GetDirectionsMapHtml(originAddress, destinationAddress);
        return embedMapHtml;
    }
}
