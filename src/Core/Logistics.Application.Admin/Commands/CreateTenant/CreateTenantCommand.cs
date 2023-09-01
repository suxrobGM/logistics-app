using System.ComponentModel.DataAnnotations;
using Logistics.Domain.Constraints;

namespace Logistics.Application.Admin.Commands;

public class CreateTenantCommand : Request<ResponseResult>
{
    [Required, StringLength(TenantConsts.NameLength)]
    public string? Name { get; set; }
    
    [StringLength(TenantConsts.DisplayNameLength)]
    public string? DisplayName { get; set; }
}
