using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace Logistics.IdentityServer.Pages.Account.LoginWithRecoveryCode;

public class InputModel
{
    [BindProperty]
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; }
}
