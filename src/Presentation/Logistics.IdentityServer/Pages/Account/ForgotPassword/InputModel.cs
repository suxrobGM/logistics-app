using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.ForgotPassword;

public class InputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
