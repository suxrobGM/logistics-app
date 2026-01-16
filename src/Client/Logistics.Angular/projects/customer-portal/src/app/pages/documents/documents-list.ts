import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Api,
  type DocumentDto,
  getPortalLoadDocuments,
  getPortalLoads,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";

interface DocumentWithLoad extends DocumentDto {
  loadNumber?: number;
  loadId?: string;
  loadName?: string | null;
}

@Component({
  selector: "cp-documents-list",
  templateUrl: "./documents-list.html",
  imports: [DatePipe, RouterLink, TableModule, ButtonModule, TagModule, ProgressSpinnerModule],
})
export class DocumentsList {
  private readonly api = inject(Api);

  protected readonly documents = signal<DocumentWithLoad[]>([]);
  protected readonly isLoading = signal(true);

  constructor() {
    this.loadDocuments();
  }

  private async loadDocuments(): Promise<void> {
    this.isLoading.set(true);
    try {
      // Get all loads first
      const loadsResult = await this.api.invoke(getPortalLoads, { PageSize: 100 });
      const loads = loadsResult.items ?? [];

      // Fetch documents for each load
      const allDocs: DocumentWithLoad[] = [];
      for (const load of loads) {
        if (load.id && (load.documentCount ?? 0) > 0) {
          const docs = await this.api.invoke(getPortalLoadDocuments, { loadId: load.id });
          if (docs) {
            for (const doc of docs) {
              allDocs.push({
                ...doc,
                loadNumber: load.number,
                loadId: load.id,
                loadName: load.name,
              });
            }
          }
        }
      }

      // Sort by created date descending
      allDocs.sort((a, b) => {
        const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0;
        const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0;
        return dateB - dateA;
      });

      this.documents.set(allDocs);
    } catch (error) {
      console.error("Failed to load documents:", error);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getDocumentTypeLabel(type: string | undefined): string {
    switch (type) {
      case "BillOfLading":
        return "Bill of Lading";
      case "ProofOfDelivery":
        return "Proof of Delivery";
      case "RateConfirmation":
        return "Rate Confirmation";
      default:
        return type ?? "Document";
    }
  }
}
