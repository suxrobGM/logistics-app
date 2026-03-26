using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDispatchSessionByIdHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetDispatchSessionByIdQuery, Result<DispatchSessionDto>>
{
    public async Task<Result<DispatchSessionDto>> Handle(
        GetDispatchSessionByIdQuery request, CancellationToken ct)
    {
        var session = await tenantUow.Repository<DispatchSession>()
            .GetByIdAsync(request.SessionId);

        if (session is null)
            return Result<DispatchSessionDto>.Fail("Session not found");

        var dto = new DispatchSessionDto
        {
            Id = session.Id,
            Number = session.Number,
            Mode = session.Mode,
            Status = session.Status,
            TriggeredByUserId = session.TriggeredByUserId,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            TotalTokensUsed = session.TotalTokensUsed,
            DecisionCount = session.DecisionCount,
            Summary = session.Summary,
            ErrorMessage = session.ErrorMessage,
            Decisions = session.Decisions.Select(d => new DispatchDecisionDto
            {
                Id = d.Id,
                SessionId = d.SessionId,
                Type = d.Type,
                Status = d.Status,
                Reasoning = d.Reasoning,
                ToolName = d.ToolName,
                ToolInput = d.ToolInput,
                ToolOutput = d.ToolOutput,
                LoadId = d.LoadId,
                TruckId = d.TruckId,
                TripId = d.TripId,
                CreatedAt = d.CreatedAt,
                ExecutedAt = d.ExecutedAt,
                ApprovedByUserId = d.ApprovedByUserId,
                RejectionReason = d.RejectionReason
            }).ToList()
        };

        return Result<DispatchSessionDto>.Ok(dto);
    }
}
