using Duende.IdentityServer.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Logistics.IdentityServer.Pages.Account.LoginWith2fa;

public class InputModel
{
    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Authenticator code")]
    public string TwoFactorCode { get; set; }

    [Display(Name = "Remember this machine")]
    public bool RememberMachine { get; set; }
}
