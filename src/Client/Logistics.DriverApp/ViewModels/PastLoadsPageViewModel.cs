using System.Collections.ObjectModel;

using Logistics.DriverApp.Consts;
using Logistics.DriverApp.Models;
using Logistics.DriverApp.Services;

namespace Logistics.DriverApp.ViewModels;

public class PastLoadsPageViewModel : BaseViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ICache _cache;

    public PastLoadsPageViewModel(
        IApiClient apiClient,
        ICache cache)
    {
        _apiClient = apiClient;
        _cache = cache;
    }


    #region Bindable properties

    public ObservableCollection<LoadDto> Loads { get; } = new();

    private LoadDto? _selectedLoad;
    public LoadDto? SelectedLoad
    {
        get => _selectedLoad;
        set
        {
            if (SetProperty(ref _selectedLoad, value) && value != null)
            {
                _ = OpenLoadPageAsync();
            }
        }
    }

    #endregion


    protected override async Task OnInitializedAsync()
    {
        await FetchLoadsAsync();
    }

    private async Task FetchLoadsAsync()
    {
        IsLoading = true;
        var truckId = _cache.Get<string>(CacheKeys.TruckId);

        if (string.IsNullOrEmpty(truckId))
        {
            await PopupHelpers.ShowErrorAsync("Could not fetch loads, the truck ID is null");
            IsLoading = false;
            return;
        }

        var past90Days = PredefinedDateRanges.Past90Days;

        var result = await _apiClient.GetLoadsAsync(new GetLoadsQuery
        {
            LoadAllPages = true,
            TruckId = truckId,
            StartDate = past90Days.StartDate,
            EndDate = past90Days.EndDate
        });

        if (!result.Success)
        {
            await PopupHelpers.ShowErrorAsync($"Failed to fetch the loads from server.\nError: {result.Error}");
            IsLoading = false;
            return;
        }

        Loads.Clear();
        foreach (var load in result.Data!)
        {
            Loads.Add(load);
        }

        IsLoading = false;
    }

    private async Task OpenLoadPageAsync()
    {
        if (SelectedLoad is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object> { { "load", SelectedLoad } };
        await Shell.Current.GoToAsync("LoadPage", parameters);
    }
}
