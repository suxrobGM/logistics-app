namespace Logistics.Application.Contracts.Commands;

public sealed class CreateTenantCommand : RequestBase<DataResult>
{
    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }
    
    [StringLength(TenantConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
    
    [Required]
    public string? ConnectionString { get; set; }
}
