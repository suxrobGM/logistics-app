import { Component, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { Api, deleteDemoRequest, updateDemoRequest } from "@logistics/shared/api";
import type { DemoRequestDto, DemoRequestStatus } from "@logistics/shared/api";
import { DataContainer, LabeledField, PageHeader, SearchInput } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TextareaModule } from "primeng/textarea";
import { TooltipModule } from "primeng/tooltip";
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
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    DataContainer,
    PageHeader,
    SearchInput,
    TagModule,
    DialogModule,
    SelectModule,
    TextareaModule,
    ReactiveFormsModule,
    LabeledField,
  ],
})
export class DemoRequestsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(DemoRequestsListStore);

  protected viewDialogVisible = signal(false);
  protected selectedRequest = signal<DemoRequestDto | null>(null);

  protected readonly form = new FormGroup({
    status: new FormControl<DemoRequestStatus>("new", { nonNullable: true }),
    notes: new FormControl<string>("", { nonNullable: true }),
  });

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
    this.form.patchValue({
      status: request.status ?? "new",
      notes: request.notes ?? "",
    });
    this.viewDialogVisible.set(true);
  }

  protected async saveRequest(): Promise<void> {
    const request = this.selectedRequest();
    if (!request?.id) return;

    const formValue = this.form.getRawValue();
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

  protected getStatusSeverity(
    status?: DemoRequestStatus,
  ): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
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

  protected formatDate(date?: string): string {
    if (!date) return "-";
    return new Date(date).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  }
}
