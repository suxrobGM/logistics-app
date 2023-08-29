using System.ComponentModel.DataAnnotations;
using Logistics.Domain.Shared.Constraints;

namespace Logistics.Application.Admin.Commands;

public sealed class CreateTenantCommand : RequestBase<ResponseResult>
{
    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }
    
    [StringLength(TenantConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
}
