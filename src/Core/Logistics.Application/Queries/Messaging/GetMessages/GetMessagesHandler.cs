using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetMessagesHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetMessagesQuery, Result<MessageDto[]>>
{
    public async Task<Result<MessageDto[]>> Handle(GetMessagesQuery req, CancellationToken ct)
    {
        // Verify conversation exists
        var conversation = await tenantUow.Repository<Conversation>()
            .GetByIdAsync(req.ConversationId, ct);

        if (conversation is null)
        {
            return Result<MessageDto[]>.Fail($"Conversation with ID '{req.ConversationId}' not found");
        }

        var messages = await tenantUow.Repository<Message>()
            .Query()
            .Where(m => m.ConversationId == req.ConversationId
                        && !m.IsDeleted
                        && (!req.Before.HasValue || m.SentAt < req.Before.Value))
            .Include(m => m.Sender)
            .Include(m => m.ReadReceipts)
            .ToListAsync(ct);

        var messageDtos = messages
            .OrderByDescending(m => m.SentAt)
            .Skip(req.Offset)
            .Take(req.Limit)
            .Select(m => m.ToDtoWithReadStatus())
            .Reverse() // Return in chronological order
            .ToArray();

        return Result<MessageDto[]>.Ok(messageDtos);
    }
}
