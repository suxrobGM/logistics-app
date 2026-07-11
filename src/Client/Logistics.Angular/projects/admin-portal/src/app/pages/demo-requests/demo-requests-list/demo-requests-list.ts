import { Component, inject, signal } from "@angular/core";
import { form, FormField } from "@angular/forms/signals";
import {
  Api,
  deleteDemoRequest,
  updateDemoRequest,
  type DemoRequestDto,
  type DemoRequestStatus,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  DataContainer,
  Grid,
  PageHeader,
  SearchField,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiDataTable,
  UiDialog,
  UiFormField,
  UiSelectField,
  UiSortHeader,
  UiTextareaField,
  UiTooltip,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { DateUtils } from "@logistics/shared/utils";
import { ToastService } from "@/core/services";
import { DemoRequestsListStore } from "../store/demo-requests-list.store";

interface StatusOption {
  label: string;
  value: DemoRequestStatus;
}

@Component({
  selector: "adm-demo-requests-list",
  templateUrl: "./demo-requests-list.html",
  providers: [DemoRequestsListStore],
  imports: [
    Badge,
    Card,
    DataContainer,
    FormField,
    Grid,
    PageHeader,
    SearchField,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiDataTable,
    UiDialog,
    UiFormField,
    UiSelectField,
    UiSortHeader,
    UiTextareaField,
    UiTooltip,
  ],
})
export class DemoRequestsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(DemoRequestsListStore);

  protected viewDialogVisible = signal(false);
  protected selectedRequest = signal<DemoRequestDto | null>(null);

  protected readonly model = signal<{ status: DemoRequestStatus; notes: string }>({
    status: "new",
    notes: "",
  });

  protected readonly form = form(this.model);

  protected readonly statusOptions: StatusOption[] = [
    { label: "New", value: "new" },
    { label: "Contacted", value: "contacted" },
    { label: "Converted", value: "converted" },
    { label: "Closed", value: "closed" },
  ];

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected viewRequest(request: DemoRequestDto): void {
    this.selectedRequest.set(request);
    this.model.set({
      status: request.status ?? "new",
      notes: request.notes ?? "",
    });
    this.viewDialogVisible.set(true);
  }

  protected async saveRequest(): Promise<void> {
    const request = this.selectedRequest();
    if (!request?.id) return;

    const formValue = this.model();
    await this.api.invoke(updateDemoRequest, {
      id: request.id,
      body: {
        status: formValue.status,
        notes: formValue.notes || null,
      },
    });

    this.toastService.showSuccess("Demo request updated successfully");
    this.viewDialogVisible.set(false);
    this.store.retry();
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirmDelete("demo request", () => this.deleteRequest(id));
  }

  private async deleteRequest(id: string): Promise<void> {
    await this.api.invoke(deleteDemoRequest, { id });
    this.toastService.showSuccess("Demo request deleted successfully");
    this.store.removeItem(id);
  }

  protected getStatusLabel(status?: DemoRequestStatus): string {
    switch (status) {
      case "new":
        return "New";
      case "contacted":
        return "Contacted";
      case "converted":
        return "Converted";
      case "closed":
        return "Closed";
      default:
        return "New";
    }
  }

  protected getStatusSeverity(status?: DemoRequestStatus): UiBadgeIntent {
    switch (status) {
      case "new":
        return "info";
      case "contacted":
        return "warn";
      case "converted":
        return "success";
      case "closed":
        return "secondary";
      default:
        return "info";
    }
  }

  protected readonly formatDate = DateUtils.formatDateTime;
}
