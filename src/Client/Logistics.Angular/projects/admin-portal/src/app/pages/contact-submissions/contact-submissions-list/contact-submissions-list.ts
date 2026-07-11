import { Component, inject, signal } from "@angular/core";
import { form, FormField } from "@angular/forms/signals";
import {
  Api,
  deleteContactSubmission,
  updateContactSubmission,
  type ContactSubject,
  type ContactSubmissionDto,
  type ContactSubmissionStatus,
} from "@logistics/shared/api";
import type { SelectOption } from "@logistics/shared/models";
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
import { ContactSubmissionsListStore } from "../store/contact-submissions-list.store";

@Component({
  selector: "adm-contact-submissions-list",
  templateUrl: "./contact-submissions-list.html",
  providers: [ContactSubmissionsListStore],
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
export class ContactSubmissionsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(ContactSubmissionsListStore);

  protected viewDialogVisible = signal(false);
  protected selectedSubmission = signal<ContactSubmissionDto | null>(null);

  protected readonly model = signal<{ status: ContactSubmissionStatus; notes: string }>({
    status: "new",
    notes: "",
  });

  protected readonly form = form(this.model);

  protected readonly statusOptions: SelectOption<ContactSubmissionStatus>[] = [
    { label: "New", value: "new" },
    { label: "In Progress", value: "in_progress" },
    { label: "Resolved", value: "resolved" },
    { label: "Closed", value: "closed" },
  ];

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected viewSubmission(submission: ContactSubmissionDto): void {
    this.selectedSubmission.set(submission);
    this.model.set({
      status: submission.status ?? "new",
      notes: submission.notes ?? "",
    });
    this.viewDialogVisible.set(true);
  }

  protected async saveSubmission(): Promise<void> {
    const submission = this.selectedSubmission();
    if (!submission?.id) return;

    const formValue = this.model();
    await this.api.invoke(updateContactSubmission, {
      id: submission.id,
      body: {
        status: formValue.status,
        notes: formValue.notes || null,
      },
    });

    this.toastService.showSuccess("Contact submission updated successfully");
    this.viewDialogVisible.set(false);
    this.store.retry();
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirmDelete("contact submission", () => this.deleteSubmission(id));
  }

  private async deleteSubmission(id: string): Promise<void> {
    await this.api.invoke(deleteContactSubmission, { id });
    this.toastService.showSuccess("Contact submission deleted successfully");
    this.store.removeItem(id);
  }

  protected getSubjectLabel(subject?: ContactSubject): string {
    switch (subject) {
      case "general":
        return "General";
      case "sales":
        return "Sales";
      case "support":
        return "Support";
      case "partnership":
        return "Partnership";
      case "press":
        return "Press";
      default:
        return "General";
    }
  }

  protected getStatusLabel(status?: ContactSubmissionStatus): string {
    switch (status) {
      case "new":
        return "New";
      case "in_progress":
        return "In Progress";
      case "resolved":
        return "Resolved";
      case "closed":
        return "Closed";
      default:
        return "New";
    }
  }

  protected getStatusSeverity(status?: ContactSubmissionStatus): UiBadgeIntent {
    switch (status) {
      case "new":
        return "info";
      case "in_progress":
        return "warn";
      case "resolved":
        return "success";
      case "closed":
        return "secondary";
      default:
        return "info";
    }
  }

  protected readonly formatDate = DateUtils.formatDateTime;
}
