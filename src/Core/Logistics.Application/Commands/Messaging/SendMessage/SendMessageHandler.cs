using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Application.Services.Realtime;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;

namespace Logistics.Application.Commands;

internal sealed class SendMessageHandler(
    ITenantUnitOfWork tenantUow,
    IRealtimeMessagingService messagingService,
    IPushNotificationService pushNotificationService)
    : IAppRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand req, CancellationToken ct)
    {
        var conversationRepo = tenantUow.Repository<Conversation>();
        var participantRepo = tenantUow.Repository<ConversationParticipant>();

        // Validate conversation exists
        var conversation = await conversationRepo.GetByIdAsync(req.ConversationId, ct);

        if (conversation is null)
        {
            return Result<MessageDto>.Fail($"Conversation with ID '{req.ConversationId}' not found");
        }

        // Check if sender is a participant (query DB directly to avoid stale data)
        var existingParticipant = await participantRepo.GetAsync(
            p => p.ConversationId == req.ConversationId && p.EmployeeId == req.SenderId, ct);

        if (existingParticipant is null)
        {
            if (conversation.IsTenantChat)
            {
                // Auto-add sender as participant for tenant chat
                var newParticipant = new ConversationParticipant
                {
                    ConversationId = conversation.Id,
                    EmployeeId = req.SenderId
                };
                await participantRepo.AddAsync(newParticipant, ct);
            }
            else
            {
                return Result<MessageDto>.Fail("Sender is not a participant of this conversation");
            }
        }

        // Get sender info
        var sender = await tenantUow.Repository<Employee>().GetByIdAsync(req.SenderId, ct);

        if (sender is null)
        {
            return Result<MessageDto>.Fail($"Sender with ID '{req.SenderId}' not found");
        }

        // Create message
        var message = Message.Create(req.ConversationId, req.SenderId, req.Content);
        await tenantUow.Repository<Message>().AddAsync(message, ct);

        // Update conversation's last message timestamp
        conversation.LastMessageAt = message.SentAt;

        await tenantUow.SaveChangesAsync(ct);

        var messageDto = message.ToDto(false);

        // Broadcast message to conversation group
        await messagingService.BroadcastMessageToConversationAsync(
            req.ConversationId,
            messageDto,
            ct);

        // Send push notifications to participants
        foreach (var participant in conversation.Participants.Where(p => p.EmployeeId != req.SenderId))
        {
            await NotifyParticipantNewMessageAsync(sender, participant, message);
        }

        return Result<MessageDto>.Ok(messageDto);
    }

    private async Task NotifyParticipantNewMessageAsync(Employee sender, ConversationParticipant participant, Message message)
    {
        if (string.IsNullOrEmpty(participant.Employee?.DeviceToken) || participant.IsMuted)
        {
            return;
        }

        await pushNotificationService.SendNotificationAsync(
            sender.GetFullName(),
            message.Content.Length > 100 ? message.Content[..100] + "..." : message.Content,
            participant.Employee.DeviceToken,
            new Dictionary<string, string>
            {
                ["type"] = "message",
                ["conversationId"] = message.ConversationId.ToString(),
                ["messageId"] = message.Id.ToString()
            });
    }
}
