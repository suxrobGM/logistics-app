using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.Manage.EnableAuthenticator;

public class InputModel
{
    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Verification code")]
    public string Code { get; set; } = string.Empty;
}
