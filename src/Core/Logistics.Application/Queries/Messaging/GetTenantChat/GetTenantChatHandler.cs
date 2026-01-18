using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Queries;

internal sealed class GetTenantChatHandler(
    ITenantUnitOfWork tenantUow,
    ITenantService tenantService)
    : IAppRequestHandler<GetTenantChatQuery, Result<ConversationDto>>
{
    public async Task<Result<ConversationDto>> Handle(GetTenantChatQuery req, CancellationToken ct)
    {
        var conversationRepo = tenantUow.Repository<Conversation>();
        var participantRepo = tenantUow.Repository<ConversationParticipant>();

        // Try to find existing tenant chat
        var tenantChat = await conversationRepo.GetAsync(c => c.IsTenantChat, ct);

        // Create if it doesn't exist
        if (tenantChat is null)
        {
            var tenant = tenantService.GetCurrentTenant();
            tenantChat = Conversation.CreateTenantChat(tenant.CompanyName ?? tenant.Name);
            await conversationRepo.AddAsync(tenantChat, ct);
            await tenantUow.SaveChangesAsync(ct);
        }

        // Ensure requesting employee is a participant (check directly in DB to avoid stale data)
        var existingParticipant = await participantRepo.GetAsync(
            p => p.ConversationId == tenantChat.Id && p.EmployeeId == req.EmployeeId, ct);

        if (existingParticipant is null)
        {
            var newParticipant = new ConversationParticipant
            {
                ConversationId = tenantChat.Id,
                EmployeeId = req.EmployeeId
            };
            await participantRepo.AddAsync(newParticipant, ct);
            await tenantUow.SaveChangesAsync(ct);
        }

        // Calculate unread count and last message
        var lastMessage = tenantChat.Messages
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        var participant = tenantChat.Participants
            .FirstOrDefault(p => p.EmployeeId == req.EmployeeId);

        var unreadCount = 0;
        if (participant?.LastReadAt.HasValue == true)
        {
            unreadCount = tenantChat.Messages
                .Count(m => m.SentAt > participant.LastReadAt.Value && m.SenderId != req.EmployeeId);
        }
        else
        {
            unreadCount = tenantChat.Messages.Count(m => m.SenderId != req.EmployeeId);
        }

        return Result<ConversationDto>.Ok(tenantChat.ToDto(unreadCount, lastMessage?.ToDto()));
    }
}
