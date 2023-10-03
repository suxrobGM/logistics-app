namespace Logistics.DriverApp.Services;

public class GoogleMapsService : IMapsService
{
    public string GetDirectionsMapHtml(string origin, string destination)
    {
        var htmlContent = $$"""
                              <!DOCTYPE html>
                              <html>
                              <head>
                                  <style>
                                      body, html {
                                          margin: 0;
                                          overflow: hidden;
                                      }
                                      .spinner-container {
                                          display: flex;
                                          justify-content: center;
                                          align-items: center;
                                          position: absolute;
                                          top: 0;
                                          left: 0;
                                          height: 100vh;
                                          width: 100vw;
                                          background: rgba(255, 255, 255, 0.5);
                                      }
                                      .spinner {
                                          border: 16px solid #f3f3f3;
                                          border-top: 16px solid #3498db;
                                          border-radius: 50%;
                                          width: 120px;
                                          height: 120px;
                                          animation: spin 2s linear infinite;
                                      }
                                      @keyframes spin {
                                          0% { transform: rotate(0deg); }
                                          100% { transform: rotate(360deg); }
                                      }
                                  </style>
                              </head>
                              <body>
                                  <div class="spinner-container">
                                      <div class="spinner"></div>
                                  </div>
                                  <iframe onload="document.querySelector('.spinner-container').style.display='none'"
                                      loading="lazy"
                                      style="border:0; height: 100vh; width: 100vw"
                                      referrerpolicy="no-referrer-when-downgrade"
                                      src="https://www.google.com/maps/embed/v1/directions?key=AIzaSyDSwTWBzwf_pnQrRrLuGy7L_URq3t6kDh0
                                        &origin={{origin}}
                                        &destination={{destination}}">
                                  </iframe>
                              </body>
                              </html>
                            """;
        return htmlContent;
    }
}
