namespace Logistics.Application.Contracts.Models;

public class TenantDto
{
    public string? Id { get; set; }

    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }

    [StringLength(TenantConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
    public string? ConnectionString { get; set; }
}
