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
    }

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
        await FetchLoadAsync();
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

    private string GetEmbedMapHtml(LoadDto load)
    {
        var originAddress = $"{load.OriginAddressLat},{load.OriginAddressLong}";
        var destinationAddress = $"{load.DestinationAddressLat},{load.DestinationAddressLong}";
        var embedMapHtml = _mapsService.GetDirectionsMapHtml(originAddress, destinationAddress);
        return embedMapHtml;
    }
}
