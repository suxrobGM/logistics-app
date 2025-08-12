using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.ResendEmailConfirmation;

public class InputModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
