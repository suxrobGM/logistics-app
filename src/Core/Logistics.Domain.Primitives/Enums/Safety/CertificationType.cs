using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum CertificationType
{
    [Description("Commercial Driver's License (CDL)")] [EnumMember(Value = "cdl")]
    Cdl,

    [Description("DOT Medical Certificate")] [EnumMember(Value = "medical_certificate")]
    MedicalCertificate,

    [Description("Hazmat Endorsement")] [EnumMember(Value = "hazmat")]
    HazmatEndorsement,

    [Description("TWIC Card")] [EnumMember(Value = "twic")]
    TwicCard,

    [Description("Tanker Endorsement")] [EnumMember(Value = "tanker")]
    TankerEndorsement,

    [Description("Doubles/Triples Endorsement")] [EnumMember(Value = "doubles_triples")]
    DoublesTriples,

    [Description("Passenger Endorsement")] [EnumMember(Value = "passenger")]
    PassengerEndorsement,

    [Description("School Bus Endorsement")] [EnumMember(Value = "school_bus")]
    SchoolBusEndorsement,

    [Description("Entry-Level Driver Training (ELDT)")] [EnumMember(Value = "eldt")]
    Eldt,

    [Description("Defensive Driving Certificate")] [EnumMember(Value = "defensive_driving")]
    DefensiveDriving,

    [Description("Hazmat Training Certificate")] [EnumMember(Value = "hazmat_training")]
    HazmatTraining,

    [Description("Other Certification")] [EnumMember(Value = "other")]
    Other
}
