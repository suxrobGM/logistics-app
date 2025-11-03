using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Logistics.DriverApp.Models;

public class AccountInfo
{
    [Required]
    [ReadOnly(true)]
    public string? Email { get; set; }

    [Required, StringLength(32)]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Required, StringLength(32)]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Display(Name = "Phone Number")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
}
