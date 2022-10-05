using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Logistics.IdentityServer.Pages.Account.Manage.Email
{

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; }
    }
}
