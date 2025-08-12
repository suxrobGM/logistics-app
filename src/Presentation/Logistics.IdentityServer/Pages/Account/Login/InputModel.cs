using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.Login;

public class InputModel
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    public bool RememberLogin { get; set; }

    public string ReturnUrl { get; set; } = "/";

    public string Button { get; set; }
}
