namespace Logistics.DriverApp.Services;

public class GoogleMapsService : IMapsService
{
    public string GetDirectionsMapHtml(string origin, string destination)
    {
        var htmlContent = $"""
                          <!DOCTYPE html>
                          <html>
                          <head></head>
                          <body style="margin: 0">
                              <iframe loading="lazy"
                                style="border:0; height: 100vh; width: 100vw"
                                referrerpolicy="no-referrer-when-downgrade"
                                src="https://www.google.com/maps/embed/v1/directions?key=AIzaSyDSwTWBzwf_pnQrRrLuGy7L_URq3t6kDh0
                                  &origin={origin}
                                  &destination={destination}">
                              </iframe>
                          </body>
                          </html>
                          """;
        return htmlContent;
    }
}
