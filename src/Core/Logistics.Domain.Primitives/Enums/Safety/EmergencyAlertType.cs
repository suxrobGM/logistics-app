using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum EmergencyAlertType
{
    [Description("Panic Button")] [EnumMember(Value = "panic_button")]
    PanicButton,

    [Description("Crash Detected")] [EnumMember(Value = "crash_detected")]
    CrashDetected,

    [Description("Rollover Detected")] [EnumMember(Value = "rollover_detected")]
    RolloverDetected,

    [Description("Airbag Deployed")] [EnumMember(Value = "airbag_deployed")]
    AirbagDeployed,

    [Description("Medical Emergency")] [EnumMember(Value = "medical_emergency")]
    MedicalEmergency,

    [Description("Security Threat")] [EnumMember(Value = "security_threat")]
    SecurityThreat,

    [Description("Vehicle Disabled")] [EnumMember(Value = "vehicle_disabled")]
    VehicleDisabled,

    [Description("Other Emergency")] [EnumMember(Value = "other")]
    Other
}
