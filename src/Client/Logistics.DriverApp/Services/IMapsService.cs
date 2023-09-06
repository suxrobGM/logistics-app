namespace Logistics.DriverApp.Services;

public interface IMapsService
{
    public string GetDirectionsMapHtml(string origin, string destination);
}
