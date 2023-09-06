using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Tenant.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly FirebaseApp _firebaseApp;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;
        _firebaseApp = FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile("firebase-adminsdk-key.json")
        });
    }
    
    public async Task SendNotificationAsync(string title, string body, string deviceToken)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };
            await FirebaseMessaging.GetMessaging(_firebaseApp).SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not send a notification to device: {DeviceToken}\nRaised an exception: {Exception}", deviceToken, ex.ToString());
        }
    }
}
