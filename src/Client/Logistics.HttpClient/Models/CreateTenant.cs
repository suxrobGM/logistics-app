using System.ComponentModel.DataAnnotations;
using Logistics.HttpClient.Constants;

namespace Logistics.HttpClient.Models;

public record CreateTenant
{
    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }

    [StringLength(TenantConsts.DisplayNameLength)]
    public string? CompanyName { get; set; }
}
