using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class UpdateTenantTaxRateCommand : ICrossDatabaseCommand<Result<TenantTaxRateDto>>
{
    public Guid Id { get; set; }
    public required decimal RatePercent { get; set; }
    public string? Description { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
