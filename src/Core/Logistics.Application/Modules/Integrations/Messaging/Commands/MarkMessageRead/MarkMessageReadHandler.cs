using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Realtime;

namespace Logistics.Application.Modules.Integrations.Messaging.Commands;

internal sealed class MarkMessageReadHandler(
    ITenantUnitOfWork tenantUow,
    IRealtimeMessagingService messagingService)
    : IRequestHandler<MarkMessageReadCommand, Result>
{
    public async Task<Result> Handle(MarkMessageReadCommand req, CancellationToken ct)
    {
        var message = await tenantUow.Repository<Message>()
            .GetByIdAsync(req.MessageId, ct);

        if (message is null)
        {
            return Result.Fail($"Message with ID '{req.MessageId}' not found");
        }

        var participant = await tenantUow.Repository<ConversationParticipant>()
            .GetAsync(p => p.ConversationId == message.ConversationId && p.EmployeeId == req.ReadById, ct);

        // Only a participant may leave a receipt, mirroring SendMessageHandler's gate.
        if (participant is null && !message.Conversation.IsTenantChat)
        {
            return Result.Fail("Reader is not a participant of this conversation");
        }

        var existingReceipt = await tenantUow.Repository<MessageReadReceipt>()
            .GetAsync(r => r.MessageId == req.MessageId && r.ReadById == req.ReadById, ct);

        if (existingReceipt is not null)
        {
            return Result.Ok(); // Already marked as read
        }

        var readReceipt = new MessageReadReceipt
        {
            MessageId = req.MessageId,
            ReadById = req.ReadById
        };

        await tenantUow.Repository<MessageReadReceipt>().AddAsync(readReceipt, ct);

        if (participant is not null)
        {
            participant.LastReadAt = DateTime.UtcNow;
        }

        await tenantUow.SaveChangesAsync(ct);

        await messagingService.BroadcastMessageReadAsync(
            message.ConversationId,
            req.MessageId,
            req.ReadById.ToString(),
            ct);

        return Result.Ok();
    }
}
