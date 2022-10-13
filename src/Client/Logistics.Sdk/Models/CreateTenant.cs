using Logistics.Sdk.Constants;
using System.ComponentModel.DataAnnotations;

namespace Logistics.Sdk.Models;

public record CreateTenant
{
    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }

    [StringLength(TenantConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
}
