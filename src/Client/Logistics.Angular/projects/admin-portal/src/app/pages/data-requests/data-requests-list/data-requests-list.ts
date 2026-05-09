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
  PageHeader,
  Stack,
  Surface,
  Typography,
  type BadgeSeverity,
} from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { ToastService } from "@/core/services";

@Component({
  selector: "adm-data-requests-list",
  templateUrl: "./data-requests-list.html",
  imports: [
    CommonModule,
    CardModule,
    ProgressSpinnerModule,
    TableModule,
    TagModule,
    PageHeader,
    Stack,
    Surface,
    Typography,
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

  protected statusSeverity(status: string | undefined): BadgeSeverity {
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
