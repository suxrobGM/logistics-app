using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Tenant.Services;

public class PushNotification : IPushNotification
{
    private readonly FirebaseApp? _firebaseApp;
    private readonly ILogger<PushNotification> _logger;

    public PushNotification(ILogger<PushNotification> logger)
    {
        _logger = logger;

        if (File.Exists("firebase-adminsdk-key.json"))
        {
            _firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase-adminsdk-key.json")
            });
        }
        else
        {
            _firebaseApp = null;
        }
    }
    
    public async Task SendNotificationAsync(
        string title, 
        string body, 
        string deviceToken, 
        IReadOnlyDictionary<string, string>? data = null)
    {
        if (_firebaseApp is null)
            return;
        
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };
            await FirebaseMessaging.GetMessaging(_firebaseApp).SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not send a notification to device: {DeviceToken}\nRaised an exception: {Exception}", deviceToken, ex.ToString());
        }
    }
}
