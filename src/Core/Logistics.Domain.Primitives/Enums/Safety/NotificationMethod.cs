using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum NotificationMethod
{
    [Description("Push")] [EnumMember(Value = "push")]
    Push,

    [Description("Sms")] [EnumMember(Value = "sms")]
    Sms,

    [Description("Phone Call")] [EnumMember(Value = "phone_call")]
    PhoneCall,

    [Description("Email")] [EnumMember(Value = "email")]
    Email
}
