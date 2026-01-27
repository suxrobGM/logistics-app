using Logistics.Application.Abstractions;
using Logistics.Application.Services.Realtime;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class MarkMessageReadHandler(
    ITenantUnitOfWork tenantUow,
    IRealtimeMessagingService messagingService)
    : IAppRequestHandler<MarkMessageReadCommand, Result>
{
    public async Task<Result> Handle(MarkMessageReadCommand req, CancellationToken ct)
    {
        // Get the message
        var message = await tenantUow.Repository<Message>()
            .GetByIdAsync(req.MessageId, ct);

        if (message is null)
        {
            return Result.Fail($"Message with ID '{req.MessageId}' not found");
        }

        // Check if already read by this user
        var existingReceipt = await tenantUow.Repository<MessageReadReceipt>()
            .GetAsync(r => r.MessageId == req.MessageId && r.ReadById == req.ReadById, ct);

        if (existingReceipt is not null)
        {
            return Result.Ok(); // Already marked as read
        }

        // Create read receipt
        var readReceipt = new MessageReadReceipt
        {
            MessageId = req.MessageId,
            ReadById = req.ReadById
        };

        await tenantUow.Repository<MessageReadReceipt>().AddAsync(readReceipt, ct);

        // Update participant's last read timestamp
        var participant = await tenantUow.Repository<ConversationParticipant>()
            .GetAsync(p => p.ConversationId == message.ConversationId && p.EmployeeId == req.ReadById, ct);

        if (participant is not null)
        {
            participant.LastReadAt = DateTime.UtcNow;
        }

        await tenantUow.SaveChangesAsync(ct);

        // Notify via SignalR
        await messagingService.BroadcastMessageReadAsync(
            message.ConversationId,
            req.MessageId,
            req.ReadById.ToString(),
            ct);

        return Result.Ok();
    }
}
