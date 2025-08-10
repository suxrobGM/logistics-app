using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum DocumentType
{
    [Description("Bill of Lading"), EnumMember(Value = "bill_of_lading")]
    BillOfLading,
    
    [Description("Proof of Delivery"), EnumMember(Value = "proof_of_delivery")]
    ProofOfDelivery,
    
    [Description("Invoice"), EnumMember(Value = "invoice")]
    Invoice,
    
    [Description("Receipt"), EnumMember(Value = "receipt")]
    Receipt,
    
    [Description("Contract"), EnumMember(Value = "contract")]
    Contract,
    
    [Description("Insurance Certificate"), EnumMember(Value = "insurance_certificate")]
    InsuranceCertificate,
    
    [Description("Photo"), EnumMember(Value = "photo")]
    Photo,
    
    [Description("Driver License"), EnumMember(Value = "driver_license")]
    DriverLicense,
    
    [Description("Vehicle Registration"), EnumMember(Value = "vehicle_registration")]
    VehicleRegistration,
    
    [Description("Identity Document"), EnumMember(Value = "identity_document")]
    IdentityDocument,
    
    [Description("Other"), EnumMember(Value = "other")]
    Other
}
