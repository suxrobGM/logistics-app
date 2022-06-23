using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Logistics.IdentityServer.Pages.Account.Manage.Index
{
    public class InputModel
    {
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }
}
