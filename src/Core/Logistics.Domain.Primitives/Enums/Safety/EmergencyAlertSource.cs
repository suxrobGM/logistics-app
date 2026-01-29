using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum EmergencyAlertSource
{
    [Description("Driver App")] [EnumMember(Value = "driver_app")]
    DriverApp,

    [Description("ELD Device")] [EnumMember(Value = "eld_device")]
    EldDevice,

    [Description("Dispatcher Initiated")] [EnumMember(Value = "dispatcher")]
    DispatcherInitiated,

    [Description("Automatic Detection")] [EnumMember(Value = "automatic")]
    AutomaticDetection
}
