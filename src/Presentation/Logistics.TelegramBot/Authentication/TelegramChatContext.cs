using Logistics.Domain.Primitives.Enums;

namespace Logistics.TelegramBot.Authentication;

internal sealed record TelegramChatContext(
    Guid TenantId,
    Guid ApiKeyId,
    TelegramChatType ChatType,
    TelegramChatRole? Role,
    string TenantName);
