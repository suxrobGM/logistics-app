namespace Logistics.Shared.Models;

public record ApiKeyDto(
    Guid Id,
    string Name,
    string KeyPrefix,
    DateTime CreatedAt,
    DateTime? LastUsedAt,
    bool IsActive);
