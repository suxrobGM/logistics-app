import { Component, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { Api, deleteContactSubmission, updateContactSubmission } from "@logistics/shared/api";
import type {
  ContactSubject,
  ContactSubmissionDto,
  ContactSubmissionStatus,
} from "@logistics/shared/api";
import { DataContainer, LabeledField, PageHeader, SearchInput } from "@logistics/shared/components";
import type { SelectOption } from "@logistics/shared/models";
import { DateUtils } from "@logistics/shared/utils";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TextareaModule } from "primeng/textarea";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { ContactSubmissionsListStore } from "../store/contact-submissions-list.store";

@Component({
  selector: "adm-contact-submissions-list",
  templateUrl: "./contact-submissions-list.html",
  providers: [ContactSubmissionsListStore],
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
export class ContactSubmissionsList {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(ContactSubmissionsListStore);

  protected viewDialogVisible = signal(false);
  protected selectedSubmission = signal<ContactSubmissionDto | null>(null);

  protected readonly form = new FormGroup({
    status: new FormControl<ContactSubmissionStatus>("new", { nonNullable: true }),
    notes: new FormControl<string>("", { nonNullable: true }),
  });

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
    this.form.patchValue({
      status: submission.status ?? "new",
      notes: submission.notes ?? "",
    });
    this.viewDialogVisible.set(true);
  }

  protected async saveSubmission(): Promise<void> {
    const submission = this.selectedSubmission();
    if (!submission?.id) return;

    const formValue = this.form.getRawValue();
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

  protected getStatusSeverity(
    status?: ContactSubmissionStatus,
  ): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
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
