using System.ComponentModel.DataAnnotations;
using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public record CreateTenant
{
    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }

    [StringLength(TenantConsts.DisplayNameLength)]
    public string? CompanyName { get; set; }
}
