using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDispatchSessionsHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetDispatchSessionsQuery, PagedResult<DispatchSessionDto>>
{
    public async Task<PagedResult<DispatchSessionDto>> Handle(
        GetDispatchSessionsQuery request, CancellationToken ct)
    {
        var totalItems = await tenantUow.Repository<DispatchSession>()
            .CountAsync(s => request.Status == null || s.Status == request.Status.Value, ct);

        var sessions = await tenantUow.Repository<DispatchSession>()
            .GetListAsync(
                s => request.Status == null || s.Status == request.Status.Value,
                ct);

        var dtos = sessions
            .OrderByDescending(s => s.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new DispatchSessionDto
            {
                Id = s.Id,
                Number = s.Number,
                Mode = s.Mode,
                Status = s.Status,
                TriggeredByUserId = s.TriggeredByUserId,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                TotalTokensUsed = s.TotalTokensUsed,
                DecisionCount = s.DecisionCount,
                Summary = s.Summary,
                ErrorMessage = s.ErrorMessage
            })
            .ToList();

        return PagedResult<DispatchSessionDto>.Succeed(dtos, totalItems, request.PageSize);
    }
}
