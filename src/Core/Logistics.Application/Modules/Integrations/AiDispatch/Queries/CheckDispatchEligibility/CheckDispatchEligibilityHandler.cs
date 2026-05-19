using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

internal sealed class CheckDispatchEligibilityHandler(IDispatchEligibilityService eligibilityService)
    : IAppRequestHandler<CheckDispatchEligibilityQuery, Result<EligibilityResultDto>>
{
    public async Task<Result<EligibilityResultDto>> Handle(
        CheckDispatchEligibilityQuery req, CancellationToken ct)
    {
        var result = await eligibilityService.CheckAsync(req.TruckId, req.LoadId, req.DriverId, ct);

        var dto = new EligibilityResultDto
        {
            IsEligible = result.IsEligible,
            Issues = result.Issues.Select(i => new EligibilityIssueDto
            {
                Code = i.Code.ToString(),
                Severity = i.Severity.ToString(),
                Message = i.Message
            }).ToList()
        };

        return Result<EligibilityResultDto>.Ok(dto);
    }
}
