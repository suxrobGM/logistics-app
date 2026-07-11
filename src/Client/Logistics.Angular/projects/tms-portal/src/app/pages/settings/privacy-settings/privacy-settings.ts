import { CommonModule } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import {
  Api,
  cancelDataDeletion,
  getConsentHistory,
  getDataExportRequest,
  getMyDataDeletions,
  getMyDataExports,
  requestDataDeletion,
  requestDataExport,
  type ConsentRecordDto,
  type DataDeletionRequestDto,
  type DataExportRequestDto,
} from "@logistics/shared/api";
import {
  Alert,
  Badge,
  Card,
  Container,
  Spinner,
  Stack,
  UiButton,
  UiDataTable,
  UiDialog,
  UiTextareaField,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader, UiFormField } from "@/shared/components";

@Component({
  selector: "app-privacy-settings",
  templateUrl: "./privacy-settings.html",
  imports: [
    Alert,
    Badge,
    Card,
    CommonModule,
    Container,
    PageHeader,
    Spinner,
    Stack,
    UiButton,
    UiDataTable,
    UiDialog,
    UiFormField,
    UiTextareaField,
  ],
})
export class PrivacySettings {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  protected readonly exports = signal<DataExportRequestDto[]>([]);
  protected readonly deletions = signal<DataDeletionRequestDto[]>([]);
  protected readonly consents = signal<ConsentRecordDto[]>([]);

  protected readonly isLoading = signal(true);
  protected readonly isRequesting = signal(false);
  protected readonly deleteDialogOpen = signal(false);
  protected readonly isDeleting = signal(false);
  protected readonly deleteReason = signal("");

  protected readonly pendingDeletion = computed(() =>
    this.deletions().find((d) => d.status === "pending"),
  );

  constructor() {
    void this.loadAll();
  }

  protected async requestExport(): Promise<void> {
    if (this.isRequesting()) return;
    this.isRequesting.set(true);
    try {
      await this.api.invoke(requestDataExport);
      this.toast.showSuccess(
        "Data export requested. We'll email you when it's ready (usually within a few minutes).",
      );
      await this.refreshExports();
    } catch {
      this.toast.showError("Could not request a data export.");
    } finally {
      this.isRequesting.set(false);
    }
  }

  protected async download(id: string): Promise<void> {
    try {
      const fresh = await this.api.invoke(getDataExportRequest, { id });
      this.exports.update((list) => list.map((e) => (e.id === fresh.id ? fresh : e)));
      if (fresh.downloadUrl) {
        window.open(fresh.downloadUrl, "_blank", "noopener,noreferrer");
      } else {
        this.toast.showError("Download link unavailable. Try again in a moment.");
      }
    } catch {
      this.toast.showError("Could not fetch the export download link.");
    }
  }

  protected openDeleteDialog(): void {
    this.deleteReason.set("");
    this.deleteDialogOpen.set(true);
  }

  protected async confirmDeletion(): Promise<void> {
    if (this.isDeleting()) return;
    this.isDeleting.set(true);
    try {
      await this.api.invoke(requestDataDeletion, {
        body: { reason: this.deleteReason().trim() || null },
      });
      this.toast.showSuccess(
        "Deletion scheduled. You have 30 days to cancel before your data is anonymized.",
      );
      this.deleteDialogOpen.set(false);
      await this.refreshDeletions();
    } catch {
      this.toast.showError("Could not schedule the deletion.");
    } finally {
      this.isDeleting.set(false);
    }
  }

  protected async cancelDeletion(id: string): Promise<void> {
    try {
      await this.api.invoke(cancelDataDeletion, { id });
      this.toast.showSuccess("Deletion request cancelled.");
      await this.refreshDeletions();
    } catch {
      this.toast.showError("Could not cancel the deletion request.");
    }
  }

  protected statusSeverity(status: string | undefined): UiBadgeIntent {
    switch (status) {
      case "ready":
      case "processed":
        return "success";
      case "pending":
      case "processing":
        return "info";
      case "failed":
        return "danger";
      case "cancelled":
      case "expired":
        return "secondary";
      default:
        return "secondary";
    }
  }

  private async loadAll(): Promise<void> {
    this.isLoading.set(true);
    try {
      await Promise.all([this.refreshExports(), this.refreshDeletions(), this.refreshConsents()]);
    } finally {
      this.isLoading.set(false);
    }
  }

  private async refreshExports(): Promise<void> {
    this.exports.set(await this.api.invoke(getMyDataExports));
  }

  private async refreshDeletions(): Promise<void> {
    this.deletions.set(await this.api.invoke(getMyDataDeletions));
  }

  private async refreshConsents(): Promise<void> {
    this.consents.set(await this.api.invoke(getConsentHistory));
  }
}
