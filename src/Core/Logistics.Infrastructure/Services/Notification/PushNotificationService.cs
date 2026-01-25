using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Logistics.Application.Services;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly FirebaseApp? firebaseApp;
    private readonly ILogger<PushNotificationService> logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        this.logger = logger;

        if (File.Exists("firebase-adminsdk-key.json"))
        {
            firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("firebase-adminsdk-key.json")
            });
        }
        else
        {
            firebaseApp = null;
        }
    }

    public async Task SendNotificationAsync(
        string title,
        string body,
        string deviceToken,
        IReadOnlyDictionary<string, string>? data = null)
    {
        if (firebaseApp is null)
        {
            return;
        }

        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };
            await FirebaseMessaging.GetMessaging(firebaseApp).SendAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError("Could not send a notification to device: {DeviceToken}\nRaised an exception: {Exception}",
                deviceToken, ex.ToString());
        }
    }
}
