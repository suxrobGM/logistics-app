using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Notifications;

namespace Logistics.Infrastructure.Communications.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly FirebaseApp? _firebaseApp;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;

        if (File.Exists("firebase-adminsdk-key.json"))
        {
            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = CredentialFactory
                    .FromFile<ServiceAccountCredential>("firebase-adminsdk-key.json")
                    .ToGoogleCredential()
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
        {
            return;
        }

        try
        {
            // The driver app registers FCM registration tokens, not Installation IDs, so Fid is not a drop-in replacement.
#pragma warning disable CS0618
            var message = new Message
            {
                Token = deviceToken,
#pragma warning restore CS0618
                Notification = new Notification { Title = title, Body = body },
                Data = data
            };
            await FirebaseMessaging.GetMessaging(_firebaseApp).SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Could not send a notification to device: {DeviceToken}\nRaised an exception: {Exception}",
                deviceToken, ex.ToString());
        }
    }
}
