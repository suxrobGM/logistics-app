using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Tax.Commands;

public class UpdateTenantTaxRateCommand : ICommand<Result<TenantTaxRateDto>>
{
    public Guid Id { get; set; }
    public required decimal RatePercent { get; set; }
    public string? Description { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
