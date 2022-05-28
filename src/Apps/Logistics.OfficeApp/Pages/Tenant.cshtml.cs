using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.OfficeApp.Pages;

[AllowAnonymous]
public class TenantModel : PageModel
{
    private readonly IApiClient _apiClient;

    public TenantModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        _apiClient.SetTenantId(Id);
        return Redirect("/");
    }
}
