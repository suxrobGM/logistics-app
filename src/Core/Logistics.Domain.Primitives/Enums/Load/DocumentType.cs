using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

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
    
    [Description("Other"), EnumMember(Value = "other")]
    Other
}