using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.AcceptInvitation;

public class InputModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First name")]
    [StringLength(32, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    [StringLength(32, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "The length of the password should be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
