using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers.Truckstop;

/// <summary>
/// Truckstop - not available yet. Its public docs (developer.truckstop.com) show what a real
/// integration needs: a signed Systems Integration Agreement (TSI@Truckstop.com) for an
/// integration id; REST auth at <c>POST https://api.truckstop.com/auth/token?scope=truckstop</c>
/// with Basic client credentials and a password grant; load search over SOAP
/// (<c>webservices.truckstop.com/v13/Searching/LoadSearch.svc</c>, <c>GetLoadSearchResults</c>);
/// and Rate Insights under <c>/modeledrate/v3</c>.
/// </summary>
internal sealed class TruckstopLoadBoardService : NotYetAvailableLoadBoardService
{
    public override LoadBoardProviderType ProviderType => LoadBoardProviderType.Truckstop;
}
