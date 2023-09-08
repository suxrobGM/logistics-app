using CommunityToolkit.Mvvm.Messaging.Messages;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.Messages;

public class SuccessfullyLoggedMessage : ValueChangedMessage<UserInfo>
{
    public SuccessfullyLoggedMessage(UserInfo value) : base(value)
    {
    }
}
