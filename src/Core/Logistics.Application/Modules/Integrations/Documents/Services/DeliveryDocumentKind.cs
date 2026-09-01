using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

/// <summary>The parts of a delivery capture that differ between a bill of lading and a proof of delivery.</summary>
public sealed record DeliveryDocumentKind(
    DocumentType DocumentType,
    string FolderName,
    string SummaryFileName,
    string ShortName,
    string FailureMessage)
{
    public static readonly DeliveryDocumentKind BillOfLading = new(
        DocumentType.BillOfLading,
        FolderName: "bol",
        SummaryFileName: "bill_of_lading.json",
        ShortName: "BOL",
        FailureMessage: "Failed to capture bill of lading.");

    public static readonly DeliveryDocumentKind ProofOfDelivery = new(
        DocumentType.ProofOfDelivery,
        FolderName: "pod",
        SummaryFileName: "proof_of_delivery.json",
        ShortName: "POD",
        FailureMessage: "Failed to capture proof of delivery.");
}
