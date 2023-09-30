using Logistics.Models;
using System.Collections.ObjectModel;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.ViewModels;

public class StatsPageViewModel : BaseViewModel
{
	private readonly IAuthService _authService;
	private readonly IApiClient _apiClient;

	public StatsPageViewModel(
		IAuthService authService,
		IApiClient apiClient)
	{
		_authService = authService;
		_apiClient = apiClient;
		_dailyGrossesStartDate = DateTime.Today.AddDays(-7);
		_dailyGrossesEndDate = DateTime.Today;
		_monthlyGrossesStartDate = new DateTime(DateTime.Now.Year, 1, 1); // beginning of the current year
		_monthlyGrossesEndDate = DateTime.Today;
		FetchTruckDailyGrossesCommand = new AsyncRelayCommand(FetchTruckDailyGrossesAsync, () => !IsLoading);
		FetchTruckMonthlyGrossesCommand = new AsyncRelayCommand(FetchTruckMonthlyGrossesAsync, () => !IsLoading);
		IsLoadingChanged += (_, _) => NotifyButtonsCanExecuteChanged();
		//ChartBrushes = new List<Brush>
		//{
		//    new SolidColorBrush(Color.FromRgb(236, 64, 122)),
		//    new SolidColorBrush(Color.FromRgb(136, 165, 211))
		//};
	}


	#region Commands

	public IAsyncRelayCommand FetchTruckDailyGrossesCommand { get; }
	public IAsyncRelayCommand FetchTruckMonthlyGrossesCommand { get; }

	#endregion


    #region Bindable properties

	// public IList<Brush> ChartBrushes { get; }
    public ObservableCollection<DailyGrossDto> DailyGrosses { get; } = new();
    public ObservableCollection<MonthlyGrossDto> MonthlyGrosses { get; } = new();
    
    private DriverStatsDto? _driverStats;
    public DriverStatsDto? DriverStats
    {
	    get => _driverStats;
	    set => SetProperty(ref _driverStats, value);
    }

    private DateTime _dailyGrossesStartDate;
    public DateTime DailyGrossesStartDate
    {
	    get => _dailyGrossesStartDate;
	    set => SetProperty(ref _dailyGrossesStartDate, value);
    }

    private DateTime _dailyGrossesEndDate;
    public DateTime DailyGrossesEndDate
    {
	    get => _dailyGrossesEndDate;
	    set => SetProperty(ref _dailyGrossesEndDate, value);
    }
    
    private DateTime _monthlyGrossesStartDate;
    public DateTime MonthlyGrossesStartDate
    {
	    get => _monthlyGrossesStartDate;
	    set => SetProperty(ref _monthlyGrossesStartDate, value);
    }

    private DateTime _monthlyGrossesEndDate;
    public DateTime MonthlyGrossesEndDate
    {
	    get => _monthlyGrossesEndDate;
	    set => SetProperty(ref _monthlyGrossesEndDate, value);
    }

    #endregion


    protected override async Task OnInitializedAsync()
    {
        await FetchTruckDailyGrossesAsync();
        await FetchTruckMonthlyGrossesAsync();
		await FetchDriverStatsAsync();
    }

    private async Task FetchDriverStatsAsync()
    {
	    IsLoading = true;
	    var driverUserId = _authService.User!.Id!;
	    var result = await _apiClient.GetDriverStatsAsync(driverUserId);

	    if (!result.Success)
	    {
		    await PopupHelpers.ShowErrorAsync("Failed to fetch driver's stats, try again");
		    IsLoading = false;
		    return;
	    }

	    DriverStats = result.Value!;
	    IsLoading = false;
    }

    private async Task FetchTruckDailyGrossesAsync()
	{
        IsLoading = true;
        var driverUserId = _authService.User!.Id!;

        var result = await _apiClient.GetDailyGrossesAsync(new GetDailyGrossesQuery
        {
	        UserId = driverUserId,
	        StartDate = DailyGrossesStartDate,
	        EndDate = DailyGrossesEndDate
        });

        if (!result.Success)
        {
	        await PopupHelpers.ShowErrorAsync("Failed to load driver's line chart data, try again");
	        IsLoading = false;
	        return;
        }

		DailyGrosses.Clear();
        foreach (var dailyGross in result.Value!.Data)
        {
	        DailyGrosses.Add(dailyGross);
        }

        IsLoading = false;
	}
    
    private async Task FetchTruckMonthlyGrossesAsync()
    {
	    IsLoading = true;
	    var driverUserId = _authService.User!.Id!;

	    var result = await _apiClient.GetMonthlyGrossesAsync(new GetMonthlyGrossesQuery
	    {
		    UserId = driverUserId,
		    StartDate = MonthlyGrossesStartDate,
		    EndDate = MonthlyGrossesEndDate
	    });

	    if (!result.Success)
	    {
		    await PopupHelpers.ShowErrorAsync("Failed to load driver bar chart data, try again");
		    IsLoading = false;
		    return;
	    }

        MonthlyGrosses.Clear();
        foreach (var monthlyGross in result.Value!.Data)
	    {
		    MonthlyGrosses.Add(monthlyGross);
	    }

	    IsLoading = false;
    }

    private void NotifyButtonsCanExecuteChanged()
    {
	    FetchTruckDailyGrossesCommand.NotifyCanExecuteChanged();
	    FetchTruckMonthlyGrossesCommand.NotifyCanExecuteChanged();
    }
}
