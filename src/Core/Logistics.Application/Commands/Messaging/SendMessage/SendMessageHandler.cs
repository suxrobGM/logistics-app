using Logistics.Application.Abstractions;
using Logistics.Application.Hubs;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Commands;

internal sealed class SendMessageHandler(
    ITenantUnitOfWork tenantUow,
    IHubContext<MessagingHub, IMessagingHubClient> messagingHub,
    IPushNotificationService pushNotificationService)
    : IAppRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand req, CancellationToken ct)
    {
        // Validate conversation exists
        var conversation = await tenantUow.Repository<Conversation>()
            .Query()
            .Include(c => c.Participants)
                .ThenInclude(p => p.Employee)
            .FirstOrDefaultAsync(c => c.Id == req.ConversationId, ct);

        if (conversation is null)
        {
            return Result<MessageDto>.Fail($"Conversation with ID '{req.ConversationId}' not found");
        }

        // Validate sender is a participant
        var isParticipant = conversation.Participants.Any(p => p.EmployeeId == req.SenderId);
        if (!isParticipant)
        {
            return Result<MessageDto>.Fail("Sender is not a participant of this conversation");
        }

        // Get sender info
        var sender = await tenantUow.Repository<Employee>().GetByIdAsync(req.SenderId, ct);
        if (sender is null)
        {
            return Result<MessageDto>.Fail($"Sender with ID '{req.SenderId}' not found");
        }

        // Create message
        var message = Message.Create(req.ConversationId, req.SenderId, req.Content);
        message.Sender = sender;
        await tenantUow.Repository<Message>().AddAsync(message, ct);

        // Update conversation's last message timestamp
        conversation.LastMessageAt = message.SentAt;

        await tenantUow.SaveChangesAsync(ct);

        var messageDto = message.ToDto(isRead: false);

        // Broadcast message to conversation participants via SignalR
        await messagingHub.Clients
            .Group($"conversation-{req.ConversationId}")
            .ReceiveMessage(messageDto);

        // Send push notifications to other participants
        foreach (var participant in conversation.Participants.Where(p => p.EmployeeId != req.SenderId))
        {
            if (!string.IsNullOrEmpty(participant.Employee?.DeviceToken) && !participant.IsMuted)
            {
                await pushNotificationService.SendNotificationAsync(
                    sender.GetFullName(),
                    message.Content.Length > 100 ? message.Content[..100] + "..." : message.Content,
                    participant.Employee.DeviceToken,
                    new Dictionary<string, string>
                    {
                        ["type"] = "message",
                        ["conversationId"] = req.ConversationId.ToString(),
                        ["messageId"] = message.Id.ToString()
                    });
            }
        }

        return Result<MessageDto>.Ok(messageDto);
    }
}
