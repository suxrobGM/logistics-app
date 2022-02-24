using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Logistics.WebApi.Client;

namespace Logistics.AdminApp.Pages;

public class HostModel : PageModel
{
    private readonly IApiClient apiClient;

    public HostModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    //public async Task<IActionResult> OnGetAsync()
    //{
    //    await apiClient.TryCreateUserAsync(User.Claims.ToUser());
    //    return Page();
    //}
}
