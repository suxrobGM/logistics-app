using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.Register;

public class InputModel
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [Display(Name = "First name")]
    [StringLength(32, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only English letters are allowed, no numbers or other symbols")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    [StringLength(32, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only English letters are allowed, no numbers or other symbols")]
    public string LastName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "The length of the password should be at least 8 characters")]
    public string Password { get; init; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
