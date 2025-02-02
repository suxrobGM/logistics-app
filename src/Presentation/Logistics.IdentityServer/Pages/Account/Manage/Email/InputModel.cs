using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.Manage.Email;

public class InputModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "New email")]
    public string NewEmail { get; set; }
}
