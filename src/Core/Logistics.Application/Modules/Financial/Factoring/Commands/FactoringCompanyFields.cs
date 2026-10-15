using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal static class FactoringCompanyFields
{
    public static void Apply(IFactoringCompanyFields fields, FactoringCompany company)
    {
        company.Name = fields.Name.Trim();
        company.RemitToAddress = fields.RemitToAddress;
        company.SubmissionEmail = fields.SubmissionEmail.Trim();
        company.Phone = fields.Phone;
        company.RecourseType = fields.RecourseType;
        company.AdvanceRatePercent = fields.AdvanceRatePercent;
        company.FeePercent = fields.FeePercent;
        company.IsDefault = fields.IsDefault;
    }

    /// <summary>Only one factoring company is the default: clear the flag on every other one.</summary>
    public static async Task ClearOtherDefaultsAsync(ITenantUnitOfWork tenantUow, Guid keepId, CancellationToken ct)
    {
        var others = await tenantUow.Repository<FactoringCompany>()
            .GetListAsync(f => f.IsDefault && f.Id != keepId, ct);
        foreach (var other in others)
        {
            other.IsDefault = false;
        }
    }
}
