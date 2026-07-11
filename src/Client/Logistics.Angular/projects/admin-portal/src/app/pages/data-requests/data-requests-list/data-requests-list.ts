import { CommonModule } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import {
  Api,
  getPendingDataRequests,
  type DataDeletionRequestDto,
  type DataExportRequestDto,
  type PendingDataRequestsDto,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  PageHeader,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiDataTable,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";

@Component({
  selector: "adm-data-requests-list",
  templateUrl: "./data-requests-list.html",
  imports: [
    Badge,
    Card,
    CommonModule,
    PageHeader,
    Spinner,
    Stack,
    Surface,
    Typography,
    UiDataTable,
  ],
})
export class DataRequestsList {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly pendingExports = signal<DataExportRequestDto[]>([]);
  protected readonly pendingDeletions = signal<DataDeletionRequestDto[]>([]);

  protected readonly hasNothing = computed(
    () =>
      !this.isLoading() &&
      this.pendingExports().length === 0 &&
      this.pendingDeletions().length === 0,
  );

  constructor() {
    void this.refresh();
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

  private async refresh(): Promise<void> {
    this.isLoading.set(true);
    try {
      const data: PendingDataRequestsDto = await this.api.invoke(getPendingDataRequests);
      this.pendingExports.set(data.pendingExports ?? []);
      this.pendingDeletions.set(data.pendingDeletions ?? []);
    } catch {
      this.toast.showError("Failed to load pending data requests.");
    } finally {
      this.isLoading.set(false);
    }
  }
}
