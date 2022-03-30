using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.OfficeApp.Pages;

public class HostModel : PageModel
{
    private readonly IApiClient apiClient;

    public HostModel(IApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = User.Claims.ToUser();
        await apiClient.TryCreateUserAsync(user);
        return Page();
    }
}
