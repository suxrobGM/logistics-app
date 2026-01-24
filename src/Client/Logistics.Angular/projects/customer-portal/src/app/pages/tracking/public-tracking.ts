import { DatePipe } from "@angular/common";
import { Component, effect, inject, input, signal } from "@angular/core";
import {
  AddressPipe,
  ErrorState,
  LoadingSkeleton,
  ToastService,
  downloadBlobFile,
} from "@logistics/shared";
import {
  Api,
  type DocumentDto,
  type PublicTrackingDto,
  downloadPublicTrackingDocument,
  getPublicTracking,
  getPublicTrackingDocuments,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { ShipmentTimeline } from "@/shared/components";
import { PublicPageLayout } from "@/shared/layout";

@Component({
  selector: "cp-public-tracking",
  templateUrl: "./public-tracking.html",
  imports: [
    DatePipe,
    ButtonModule,
    TagModule,
    TableModule,
    AddressPipe,
    ErrorState,
    LoadingSkeleton,
    PublicPageLayout,
    ShipmentTimeline,
  ],
})
export class PublicTracking {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly tenantId = input.required<string>();
  protected readonly token = input.required<string>();

  protected readonly tracking = signal<PublicTrackingDto | null>(null);
  protected readonly documents = signal<DocumentDto[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    effect(() => {
      const tenantId = this.tenantId();
      const token = this.token();
      if (tenantId && token) {
        this.loadData(tenantId, token);
      }
    });
  }

  private async loadData(tenantId: string, token: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const [trackingData, docs] = await Promise.all([
        this.api.invoke(getPublicTracking, { tenantId, token }),
        this.api.invoke(getPublicTrackingDocuments, { tenantId, token }),
      ]);
      this.tracking.set(trackingData);
      this.documents.set(docs ?? []);
    } catch (err: unknown) {
      console.error("Failed to load tracking data:", err);
      const errorMessage =
        err instanceof Error && err.message.includes("expired")
          ? "This tracking link has expired or been revoked."
          : "Unable to load tracking information. The link may be invalid or expired.";
      this.error.set(errorMessage);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getStatusSeverity(status: string | undefined): "success" | "info" | "warn" | "danger" {
    switch (status) {
      case "Delivered":
        return "success";
      case "PickedUp":
        return "info";
      case "Dispatched":
        return "warn";
      case "Cancelled":
        return "danger";
      default:
        return "info";
    }
  }

  protected async downloadDocument(doc: DocumentDto): Promise<void> {
    try {
      const blob = await this.api.invoke(downloadPublicTrackingDocument, {
        tenantId: this.tenantId(),
        token: this.token(),
        documentId: doc.id!,
      });
      downloadBlobFile(blob, doc.originalFileName || doc.fileName || "document");
    } catch (err) {
      this.toastService.showError("Failed to download document");
      console.error("Failed to download document:", err);
    }
  }
}
