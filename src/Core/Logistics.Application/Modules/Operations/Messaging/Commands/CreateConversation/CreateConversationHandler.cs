using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Modules.Operations.Messaging.Commands;

internal sealed class CreateConversationHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<CreateConversationCommand, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(CreateConversationCommand req, CancellationToken ct)
    {
        // Validate participants exist
        var employeeRepo = tenantUow.Repository<Employee>();
        foreach (var participantId in req.ParticipantIds)
        {
            var employee = await employeeRepo.GetByIdAsync(participantId, ct);
            if (employee is null)
            {
                return Result<ConversationDto>.Fail($"Employee with ID '{participantId}' not found", ErrorCodes.NotFound);
            }
        }

        // Validate load exists if specified
        if (req.LoadId.HasValue)
        {
            var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId.Value, ct);
            if (load is null)
            {
                return Result<ConversationDto>.Fail($"Load with ID '{req.LoadId}' not found", ErrorCodes.NotFound);
            }
        }

        // Create conversation
        var conversation = new Conversation
        {
            Name = req.Name,
            LoadId = req.LoadId,
            Participants = req.ParticipantIds.Select(id => new ConversationParticipant
            {
                EmployeeId = id
            }).ToList()
        };

        await tenantUow.Repository<Conversation>().AddAsync(conversation, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<ConversationDto>.Ok(conversation.ToDto());
    }
}
