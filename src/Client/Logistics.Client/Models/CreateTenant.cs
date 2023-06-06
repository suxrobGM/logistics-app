using System.ComponentModel.DataAnnotations;
using Logistics.Client.Constants;

namespace Logistics.Client.Models;

public record CreateTenant
{
    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }

    [StringLength(TenantConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
}
