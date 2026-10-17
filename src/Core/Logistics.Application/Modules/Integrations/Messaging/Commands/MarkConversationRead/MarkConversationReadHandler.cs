using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Realtime;

namespace Logistics.Application.Modules.Integrations.Messaging.Commands;

internal sealed class MarkConversationReadHandler(
    ITenantUnitOfWork tenantUow,
    IRealtimeMessagingService messagingService)
    : IRequestHandler<MarkConversationReadCommand, Result>
{
    public async Task<Result> Handle(MarkConversationReadCommand req, CancellationToken ct)
    {
        var participant = await tenantUow.Repository<ConversationParticipant>()
            .GetAsync(p => p.ConversationId == req.ConversationId && p.EmployeeId == req.ReadById, ct);

        // Only a participant may leave a receipt, mirroring SendMessageHandler's gate.
        if (participant is null)
        {
            var conversation = await tenantUow.Repository<Conversation>()
                .GetByIdAsync(req.ConversationId, ct);

            if (conversation is null)
            {
                return Result.Fail($"Conversation with ID '{req.ConversationId}' not found");
            }

            if (!conversation.IsTenantChat)
            {
                return Result.Fail("Reader is not a participant of this conversation");
            }
        }

        var unreadMessages = await tenantUow.Repository<Message>().GetListAsync(
            m => m.ConversationId == req.ConversationId
                 && m.SenderId != req.ReadById
                 && !m.IsDeleted
                 && !m.ReadReceipts.Any(r => r.ReadById == req.ReadById),
            ct);

        foreach (var message in unreadMessages)
        {
            await tenantUow.Repository<MessageReadReceipt>().AddAsync(
                new MessageReadReceipt { MessageId = message.Id, ReadById = req.ReadById }, ct);
        }

        if (participant is not null)
        {
            participant.LastReadAt = DateTime.UtcNow;
        }

        await tenantUow.SaveChangesAsync(ct);

        if (unreadMessages.Count > 0)
        {
            await messagingService.BroadcastConversationReadAsync(req.ConversationId, req.ReadById, ct);
        }

        return Result.Ok();
    }
}
