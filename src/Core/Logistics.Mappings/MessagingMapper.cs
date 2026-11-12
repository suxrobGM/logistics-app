using Logistics.Domain.Entities.Messaging;
using Logistics.Shared.Models.Messaging;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class MessagingMapper
{
    extension(Message entity)
    {
        /// <summary>
        /// Maps a Message entity to MessageDto.
        /// Note: IsRead must be set by the caller based on the requesting user's read receipts.
        /// </summary>
        [UserMapping(Default = true)]
        public MessageDto ToDto()
        {
            return new MessageDto
            {
                Id = entity.Id,
                ConversationId = entity.ConversationId,
                SenderId = entity.SenderId,
                SenderName = entity.Sender?.GetFullName(),
                Content = entity.Content,
                SentAt = entity.SentAt,
                IsRead = false // Must be set by caller
            };
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
        [UserMapping(Default = true)]
        public ConversationParticipantDto ToDto()
        {
            return new ConversationParticipantDto
            {
                EmployeeId = entity.EmployeeId,
                EmployeeName = entity.Employee?.GetFullName(),
                JoinedAt = entity.JoinedAt,
                LastReadAt = entity.LastReadAt,
                IsMuted = entity.IsMuted
            };
        }
    }

    extension(Conversation entity)
    {
        /// <summary>
        /// Maps a Conversation entity to ConversationDto.
        /// Note: UnreadCount and LastMessage must be set by the caller.
        /// </summary>
        [UserMapping(Default = true)]
        public ConversationDto ToDto()
        {
            return new ConversationDto
            {
                Id = entity.Id,
                Name = entity.Name,
                LoadId = entity.LoadId,
                IsTenantChat = entity.IsTenantChat,
                CreatedAt = entity.CreatedAt,
                LastMessageAt = entity.LastMessageAt,
                Participants = entity.Participants.Select(p => p.ToDto()).ToList(),
                UnreadCount = 0, // Must be set by caller
                LastMessage = null // Must be set by caller
            };
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
