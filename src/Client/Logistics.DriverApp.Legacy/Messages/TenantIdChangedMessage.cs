using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Logistics.DriverApp.Messages;

public class TenantIdChangedMessage : ValueChangedMessage<string>
{
    public TenantIdChangedMessage(string value) : base(value)
    {
    }
}
