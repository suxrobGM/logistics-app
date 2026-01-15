using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetConversationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetConversationsQuery, Result<ConversationDto[]>>
{
    public async Task<Result<ConversationDto[]>> Handle(GetConversationsQuery req, CancellationToken ct)
    {
        var conversationRepo = tenantUow.Repository<Conversation>();

        IEnumerable<Conversation> conversations;

        if (req.ParticipantId.HasValue)
        {
            // Get conversations where the user is a participant
            var participantConversations = await tenantUow.Repository<ConversationParticipant>()
                .Query()
                .Where(p => p.EmployeeId == req.ParticipantId.Value)
                .Include(p => p.Conversation)
                    .ThenInclude(c => c.Participants)
                        .ThenInclude(cp => cp.Employee)
                .Include(p => p.Conversation)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(m => m.Sender)
                .ToListAsync(ct);

            conversations = participantConversations
                .Select(p => p.Conversation)
                .Where(c => !req.LoadId.HasValue || c.LoadId == req.LoadId.Value);
        }
        else if (req.LoadId.HasValue)
        {
            conversations = await conversationRepo.Query()
                .Where(c => c.LoadId == req.LoadId.Value)
                .Include(c => c.Participants)
                    .ThenInclude(p => p.Employee)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .ToListAsync(ct);
        }
        else
        {
            conversations = await conversationRepo.Query()
                .Include(c => c.Participants)
                    .ThenInclude(p => p.Employee)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .ToListAsync(ct);
        }

        var conversationDtos = conversations
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => ToDto(c, req.ParticipantId))
            .ToArray();

        return Result<ConversationDto[]>.Ok(conversationDtos);
    }

    private static ConversationDto ToDto(Conversation conversation, Guid? currentUserId)
    {
        var lastMessage = conversation.Messages
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();

        var unreadCount = 0;
        if (currentUserId.HasValue)
        {
            var participant = conversation.Participants
                .FirstOrDefault(p => p.EmployeeId == currentUserId.Value);

            if (participant?.LastReadAt.HasValue == true)
            {
                unreadCount = conversation.Messages
                    .Count(m => m.SentAt > participant.LastReadAt.Value && m.SenderId != currentUserId.Value);
            }
            else
            {
                unreadCount = conversation.Messages.Count(m => m.SenderId != currentUserId.Value);
            }
        }

        return conversation.ToDto(unreadCount, lastMessage?.ToDto());
    }
}
