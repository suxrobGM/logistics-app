using Logistics.Models;
using System.Collections.ObjectModel;
using Logistics.DriverApp.Models;
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
		RefreshChartCommand = new AsyncRelayCommand(FetchChartDataAsync, () => !IsLoading);
		IsLoadingChanged += (_, _) => RefreshChartCommand.NotifyCanExecuteChanged();
		
		ChartData = new ObservableCollection<IGrossChartData>();
		ChartDateRanges = new List<DateRange>()
		{
			PredefinedDateRanges.ThisWeek,
			PredefinedDateRanges.LastWeek,
			PredefinedDateRanges.ThisMonth,
			PredefinedDateRanges.LastMonth,
			PredefinedDateRanges.Past90Days,
			PredefinedDateRanges.ThisYear,
			PredefinedDateRanges.LastYear
        };

        _chartDateRange = PredefinedDateRanges.ThisYear;
		//ChartBrushes = new List<Brush>
		//{
		//    new SolidColorBrush(Color.FromRgb(236, 64, 122)),
		//    new SolidColorBrush(Color.FromRgb(136, 165, 211))
		//};
	}


	#region Commands

	public IAsyncRelayCommand RefreshChartCommand { get; }

	#endregion


    #region Bindable properties

	// public IList<Brush> ChartBrushes { get; }
    public ObservableCollection<IGrossChartData> ChartData { get; }
    public List<DateRange> ChartDateRanges { get; }
    
    private DateRange _chartDateRange;
    public DateRange ChartDateRange
    {
	    get => _chartDateRange;
	    set => SetProperty(ref _chartDateRange, value);
    }
    
    private DriverStatsDto? _driverStats;
    public DriverStatsDto? DriverStats
    {
	    get => _driverStats;
	    set => SetProperty(ref _driverStats, value);
    }

    #endregion


    protected override async Task OnInitializedAsync()
    {
		await FetchDriverStatsAsync();
		await FetchChartDataAsync();
    }

    private async Task FetchChartDataAsync()
    {
	    if (ChartDateRange == PredefinedDateRanges.ThisYear || 
	        ChartDateRange == PredefinedDateRanges.LastYear)
	    {
		    await FetchTruckMonthlyGrossesAsync();
	    }
	    else
	    {
		    await FetchTruckDailyGrossesAsync();
	    }
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
	        StartDate = ChartDateRange.StartDate,
	        EndDate = ChartDateRange.EndDate
        });

        if (!result.Success)
        {
	        await PopupHelpers.ShowErrorAsync("Failed to load driver chart data, try again");
	        IsLoading = false;
	        return;
        }

        AddChartDataToList(result.Value!.Data);
        IsLoading = false;
	}
    
    private async Task FetchTruckMonthlyGrossesAsync()
    {
	    IsLoading = true;
	    var driverUserId = _authService.User!.Id!;

	    var result = await _apiClient.GetMonthlyGrossesAsync(new GetMonthlyGrossesQuery
	    {
		    UserId = driverUserId,
		    StartDate = ChartDateRange.StartDate,
		    EndDate = ChartDateRange.EndDate
	    });

	    if (!result.Success)
	    {
		    await PopupHelpers.ShowErrorAsync("Failed to load driver chart data, try again");
		    IsLoading = false;
		    return;
	    }
	    
        AddChartDataToList(result.Value!.Data);
	    IsLoading = false;
    }

    private void AddChartDataToList(IEnumerable<IGrossChartData> grossesChart)
    {
	    ChartData.Clear();
	    foreach (var monthlyGross in grossesChart)
	    {
		    ChartData.Add(monthlyGross);
	    }
    }
}
