using Microsoft.AspNetCore.Authentication;

namespace Logistics.OfficeApp.ViewModels.Components;

public class MainLayoutViewModel : ViewModelBase
{
    private readonly IApiClient _apiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public MainLayoutViewModel(
        IApiClient apiClient, 
        IHttpContextAccessor httpContextAccessor)
    {
        _apiClient = apiClient;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public override async Task OnInitializedAsync()
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            return;
        }
    
        if (_apiClient.AccessToken is null)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            _apiClient.AccessToken = accessToken;
        }
    }
}
