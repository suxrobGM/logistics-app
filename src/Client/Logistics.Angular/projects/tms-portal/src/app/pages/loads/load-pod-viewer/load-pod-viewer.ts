import { CommonModule, DatePipe } from "@angular/common";
import { Component, effect, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, downloadDocument, getDocuments } from "@logistics/shared/api";
import type { DocumentDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { ImageModule } from "primeng/image";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { downloadBlobFile } from "@/shared/utils";

@Component({
  selector: "app-load-pod-viewer",
  templateUrl: "./load-pod-viewer.html",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    TableModule,
    TagModule,
    TooltipModule,
    DialogModule,
    ImageModule,
    ProgressSpinnerModule,
    RouterLink,
    DatePipe,
  ],
})
export class LoadPodViewerPage {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  readonly id = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly documents = signal<DocumentDto[]>([]);
  protected readonly selectedDocument = signal<DocumentDto | null>(null);
  protected readonly showDetailDialog = signal(false);

  constructor() {
    effect(() => {
      const loadId = this.id();
      if (loadId) {
        this.loadDocuments(loadId);
      }
    });
  }

  protected refresh(): void {
    const loadId = this.id();
    if (loadId) {
      this.loadDocuments(loadId);
    }
  }

  private async loadDocuments(loadId: string): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getDocuments, {
      OwnerType: "load",
      OwnerId: loadId,
    });

    if (result) {
      // Filter to only POD and BOL documents
      const filtered = result.filter(
        (doc) => doc.type === "proof_of_delivery" || doc.type === "bill_of_lading"
      );
      this.documents.set(filtered);
    }
    this.isLoading.set(false);
  }

  protected viewDetails(doc: DocumentDto): void {
    this.selectedDocument.set(doc);
    this.showDetailDialog.set(true);
  }

  protected async download(doc: DocumentDto): Promise<void> {
    try {
      const blob = await this.api.invoke(downloadDocument, { documentId: doc.id! });
      const fileName = doc.originalFileName || doc.fileName;
      downloadBlobFile(blob, fileName!);
    } catch {
      this.toast.showError("Failed to download file");
    }
  }

  protected formatFileSize(bytes?: number): string {
    if (!bytes || bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Bytes", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  }

  protected getTypeLabel(type?: string): string {
    return type === "proof_of_delivery" ? "Proof of Delivery" : "Bill of Lading";
  }

  protected getTypeSeverity(type?: string): "success" | "info" {
    return type === "proof_of_delivery" ? "success" : "info";
  }

  protected formatCoordinates(lat: number | null | undefined, lng: number | null | undefined): string {
    if (lat == null || lng == null) return "N/A";
    return `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
  }

  protected getGoogleMapsUrl(lat: number | null | undefined, lng: number | null | undefined): string {
    if (lat == null || lng == null) return "";
    return `https://www.google.com/maps?q=${lat},${lng}`;
  }

  protected isImage(doc: DocumentDto): boolean {
    return doc.contentType?.startsWith("image/") || false;
  }
}
