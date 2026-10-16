using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.Login;

public class InputModel
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberLogin { get; set; }

    public string ReturnUrl { get; set; } = "/";

    public string? Button { get; set; }
}
