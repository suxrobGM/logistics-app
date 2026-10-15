using Logistics.Domain.Entities;

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
    }
}
