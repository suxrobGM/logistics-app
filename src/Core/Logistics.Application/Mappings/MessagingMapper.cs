using Logistics.Domain.Entities.Messaging;
using Logistics.Shared.Models.Messaging;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class MessagingMapper
{
    [MapperIgnoreSource(nameof(Message.Conversation))]
    [MapperIgnoreSource(nameof(Message.Sender))]
    [MapperIgnoreSource(nameof(Message.ReadReceipts))]
    [MapperIgnoreTarget(nameof(MessageDto.SenderName))]
    [MapperIgnoreTarget(nameof(MessageDto.IsRead))]
    private static partial MessageDto MapMessage(Message entity);

    [MapperIgnoreSource(nameof(ConversationParticipant.Conversation))]
    [MapperIgnoreSource(nameof(ConversationParticipant.Employee))]
    [MapperIgnoreTarget(nameof(ConversationParticipantDto.EmployeeName))]
    private static partial ConversationParticipantDto MapParticipant(ConversationParticipant entity);

    [MapperIgnoreSource(nameof(Conversation.Load))]
    [MapperIgnoreSource(nameof(Conversation.Participants))]
    [MapperIgnoreSource(nameof(Conversation.Messages))]
    [MapperIgnoreTarget(nameof(ConversationDto.Participants))]
    [MapperIgnoreTarget(nameof(ConversationDto.UnreadCount))]
    [MapperIgnoreTarget(nameof(ConversationDto.LastMessage))]
    private static partial ConversationDto MapConversation(Conversation entity);

    extension(Message entity)
    {
        /// <summary>
        /// Maps a Message entity to MessageDto.
        /// Note: IsRead must be set by the caller based on the requesting user's read receipts.
        /// </summary>
        public MessageDto ToDto()
        {
            var dto = MapMessage(entity);
            return dto with { SenderName = entity.Sender?.GetFullName() };
        }

        /// <summary>
        /// Maps a Message entity to MessageDto with IsRead status.
        /// </summary>
        public MessageDto ToDto(bool isRead)
        {
            var dto = entity.ToDto();
            return dto with { IsRead = isRead };
        }

        /// <summary>
        /// Maps a Message entity to MessageDto with IsRead calculated from the requesting user.
        /// </summary>
        public MessageDto ToDto(Guid requestingUserId)
        {
            var isRead = entity.ReadReceipts.Any(r => r.ReadById == requestingUserId);
            return entity.ToDto(isRead);
        }

        /// <summary>
        /// Maps a Message entity to MessageDto with IsRead based on whether any read receipts exist.
        /// </summary>
        public MessageDto ToDtoWithReadStatus()
        {
            return entity.ToDto(entity.ReadReceipts.Count > 0);
        }
    }

    extension(ConversationParticipant entity)
    {
        /// <summary>
        /// Maps a ConversationParticipant entity to ConversationParticipantDto.
        /// </summary>
        public ConversationParticipantDto ToDto()
        {
            var dto = MapParticipant(entity);
            return dto with { EmployeeName = entity.Employee?.GetFullName() };
        }
    }

    extension(Conversation entity)
    {
        /// <summary>
        /// Maps a Conversation entity to ConversationDto.
        /// Note: UnreadCount and LastMessage must be set by the caller.
        /// </summary>
        public ConversationDto ToDto()
        {
            var dto = MapConversation(entity);
            return dto with { Participants = entity.Participants.Select(p => p.ToDto()).ToList() };
        }

        /// <summary>
        /// Maps a Conversation entity to ConversationDto with unread count and last message.
        /// </summary>
        public ConversationDto ToDto(int unreadCount, MessageDto? lastMessage)
        {
            var dto = entity.ToDto();
            return dto with { UnreadCount = unreadCount, LastMessage = lastMessage };
        }
    }
}
