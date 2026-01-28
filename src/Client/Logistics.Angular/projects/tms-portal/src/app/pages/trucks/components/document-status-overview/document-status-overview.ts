import { Component, computed, input } from "@angular/core";
import type { DocumentDto, DocumentType } from "@logistics/shared/api";
import { TooltipModule } from "primeng/tooltip";

interface DocumentTypeInfo {
  type: DocumentType;
  label: string;
  required: boolean;
}

const TRUCK_DOCUMENT_TYPES: DocumentTypeInfo[] = [
  { type: "vehicle_registration", label: "Registration", required: true },
  { type: "insurance_certificate", label: "Insurance", required: true },
  { type: "dot_inspection", label: "DOT Inspection", required: true },
  { type: "annual_inspection", label: "Annual Inspection", required: true },
  { type: "title_certificate", label: "Title", required: false },
  { type: "lease_agreement", label: "Lease Agreement", required: false },
];

@Component({
  selector: "app-document-status-overview",
  templateUrl: "./document-status-overview.html",
  imports: [TooltipModule],
})
export class DocumentStatusOverview {
  public readonly documents = input<DocumentDto[]>([]);

  protected readonly documentTypes = TRUCK_DOCUMENT_TYPES;

  protected readonly summary = computed(() => {
    const docs = this.documents();
    const uploadedTypes = new Set(docs.map((d) => d.type));

    const required = TRUCK_DOCUMENT_TYPES.filter((t) => t.required);
    const uploadedRequired = required.filter((t) => uploadedTypes.has(t.type));
    const missingRequired = required.filter((t) => !uploadedTypes.has(t.type));

    return {
      totalUploaded: docs.length,
      requiredCount: required.length,
      uploadedRequiredCount: uploadedRequired.length,
      missingRequired: missingRequired,
      missingRequiredLabels: missingRequired.map((d) => d.label).join(", "),
      isComplete: missingRequired.length === 0,
    };
  });

  protected hasDocument(type: DocumentType): boolean {
    return this.documents().some((d) => d.type === type);
  }
}
