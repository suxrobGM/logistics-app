using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum DocumentType
{
    BillOfLading,
    ProofOfDelivery,
    Invoice,
    Receipt,
    Contract,
    InsuranceCertificate,
    Photo,
    DriverLicense,
    VehicleRegistration,
    IdentityDocument,
    PickupInspection,
    DeliveryInspection,
    Other,

    [Description("DOT Inspection")]
    DotInspection,

    TitleCertificate,
    LeaseAgreement,
    MaintenanceRecord,
    AnnualInspection
}
