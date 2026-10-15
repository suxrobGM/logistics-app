using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;

/// <summary>
/// 123Loadboard - not available yet. Its API docs (developers.123loadboard.com) require a
/// partner login, granted through partner-integrations@123loadboard.com; nothing public was
/// found to build against.
/// </summary>
internal sealed class OneTwo3LoadBoardService : NotYetAvailableLoadBoardService
{
    public override LoadBoardProviderType ProviderType => LoadBoardProviderType.OneTwo3Loadboard;
}
