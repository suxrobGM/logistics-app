using Microsoft.AspNetCore.Authentication;

namespace Logistics.AdminApp.Shared;

public partial class MainLayout : LayoutComponentBase
{
    #region Injectable services

    [Inject] 
    private IApiClient ApiClient { get; set; } = default!;

    [Inject] 
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    #endregion
    
    protected override async Task OnInitializedAsync()
    {
        if (HttpContextAccessor.HttpContext is null)
        {
            return;
        }
    
        if (ApiClient.AccessToken is null)
        {
            var accessToken = await HttpContextAccessor.HttpContext.GetTokenAsync("access_token");
            ApiClient.AccessToken = accessToken;
        }
    }
}