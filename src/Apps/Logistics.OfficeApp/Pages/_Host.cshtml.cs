using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.OfficeApp.Pages;

public class HostModel : PageModel
{
    private readonly IApiClient _apiClient;

    public HostModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = User.Claims.ToUser();
        await _apiClient.TryCreateEmployeeAsync(user);
        return Page();
    }
}
