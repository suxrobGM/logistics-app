namespace Logistics.Shared.Models;

public record ApiKeyCreatedDto(
    Guid Id,
    string Name,
    string KeyPrefix,
    string PlaintextKey);
