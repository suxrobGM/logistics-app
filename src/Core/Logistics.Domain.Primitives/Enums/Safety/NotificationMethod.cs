using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum NotificationMethod
{
    Push,

    [Description("SMS")]
    Sms,

    PhoneCall,
    Email
}
