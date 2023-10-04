using CommunityToolkit.Mvvm.Messaging.Messages;
using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.Messages;

public class UserLoggedInMessage : ValueChangedMessage<UserInfo>
{
    public UserLoggedInMessage(UserInfo value) : base(value)
    {
    }
}
