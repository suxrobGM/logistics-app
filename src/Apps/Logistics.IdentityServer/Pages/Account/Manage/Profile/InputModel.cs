using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Logistics.IdentityServer.Pages.Account.Manage.Profile
{
    public class InputModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Display(Name ="First name")]
        [Required]
        [StringLength(50, ErrorMessage = "The first name must contains more than 2 characters", MinimumLength = 2)]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        [Required]
        [StringLength(50, ErrorMessage = "The last name must contains more than 2 characters", MinimumLength = 2)]
        [DataType(DataType.Text)]
        public string LastName { get; set; }
    }
}
