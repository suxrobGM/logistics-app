using System.ComponentModel.DataAnnotations;

namespace Logistics.IdentityServer.Pages.Account.Register;

public class InputModel
{
    [Required, EmailAddress]
    public string Email { get; init; }
        
    [Required]
    [Display(Name = "First name")]
    [StringLength(32, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only English letters are allowed, no numbers or other symbols")]
    public string FirstName { get; set; }
        
    [Required]
    [Display(Name = "Last name")]
    [StringLength(32, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only English letters are allowed, no numbers or other symbols")]
    public string LastName { get; set; }

    [Required, DataType(DataType.Password)]
    [Display(Name = "Password")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "The length of the password should be at least 8 characters")]
    public string Password { get; init; }
        
    [Required, DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; init; }
}