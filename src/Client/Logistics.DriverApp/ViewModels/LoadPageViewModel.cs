using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services;

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
    }

    #region Bindable properties

    private ActiveLoad? _activeLoad;
    public ActiveLoad? ActiveLoad
    {
        get => _activeLoad;
        set => SetProperty(ref _activeLoad, value);
    }

    #endregion
    
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var loadId = query["loadId"] as string;
        
        if (loadId == _lastLoadId)
        {
            return;
        }

        if (string.IsNullOrEmpty(loadId))
        {
            await PopupHelpers.ShowErrorAsync("Load ID is null, try again");
            return;
        }

        _lastLoadId = loadId;
        await FetchLoadAsync(loadId);
    }

    private async Task FetchLoadAsync(string loadId)
    {
        IsLoading = true;
        var result = await _apiClient.GetLoadAsync(loadId);
        
        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync("Failed to fetch the load data, try later");
            IsLoading = false;
            return;
        }

        var loadDto = result.Value!;
        var originAddress = $"{loadDto.OriginAddressLat},{loadDto.OriginAddressLong}";
        var destinationAddress = $"{loadDto.DestinationAddressLat},{loadDto.DestinationAddressLong}";
        var embedMapHtml = _mapsService.GetDirectionsMapHtml(originAddress, destinationAddress);
        ActiveLoad = new ActiveLoad(loadDto, embedMapHtml);
        IsLoading = false;
    }
}
