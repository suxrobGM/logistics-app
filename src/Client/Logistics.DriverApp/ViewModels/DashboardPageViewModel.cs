using Logistics.Models;
using System.Collections.ObjectModel;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class DashboardPageViewModel : BaseViewModel
{
	private readonly IAuthService _authService;
	private readonly IApiClient _apiClient;

	public DashboardPageViewModel(
		IAuthService authService,
		IApiClient apiClient)
	{
		_authService = authService;
		_apiClient = apiClient;
	}


    #region Bindable properties

    public ObservableCollection<DailyGrossDto> DailyGrosses { get; } = new();


    #endregion


    protected override async Task OnInitializedAsync()
    {
        await FetchTruckDailyGrossesAsync();
    }

    private async Task FetchTruckDailyGrossesAsync()
	{
        IsLoading = true;
        var driverId = _authService.User!.Id!;
        var monthAgo = DateTime.Now.AddMonths(-1);

        var result = await _apiClient.GetDailyGrossesAsync(new GetDailyGrossesQuery
        {
	        UserId = driverId,
	        StartDate = monthAgo,
	        EndDate = DateTime.Now
        });

        if (!result.Success)
        {
	        await PopupHelpers.ShowErrorAsync("Failed to load driver grosses data, try again");
	        IsLoading = false;
	        return;
        }

        foreach (var dailyGross in result.Value!.Data)
        {
	        DailyGrosses.Add(dailyGross);
        }

        IsLoading = false;
	}
}
