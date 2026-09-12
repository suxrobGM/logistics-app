using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers;

internal static class LoadBoardWebhookResults
{
    public static LoadBoardWebhookResultDto Invalid(string error) => new()
    {
        IsValid = false,
        EventType = LoadBoardWebhookEventType.Unknown,
        ErrorMessage = error
    };
}
