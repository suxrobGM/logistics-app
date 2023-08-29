using System.ComponentModel.DataAnnotations;

namespace Logistics.DriverApp.Models;

public class AccountInfo
{
    [Required]
    public string? Email { get; set; }

    [Required, StringLength(32)]
    public string? FirstName { get; set; }
    
    [Required, StringLength(32)]
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
