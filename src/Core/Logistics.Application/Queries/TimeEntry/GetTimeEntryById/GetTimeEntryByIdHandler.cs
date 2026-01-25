using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTimeEntryByIdHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTimeEntryByIdQuery, Result<TimeEntryDto>>
{
    public async Task<Result<TimeEntryDto>> Handle(GetTimeEntryByIdQuery req, CancellationToken ct)
    {
        var timeEntry = await tenantUow.Repository<TimeEntry>().GetByIdAsync(req.Id);
        if (timeEntry is null)
        {
            return Result<TimeEntryDto>.Fail("Time entry not found");
        }

        return Result<TimeEntryDto>.Ok(timeEntry.ToDto());
    }
}
