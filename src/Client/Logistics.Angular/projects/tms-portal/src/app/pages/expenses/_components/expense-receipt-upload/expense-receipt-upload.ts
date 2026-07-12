import { Component, computed, inject, input, output, signal } from "@angular/core";
import { Api, uploadExpenseReceipt } from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";
import { Icon, Stack, UiFileUpload } from "@logistics/shared/ui";
import { UiFormField } from "@/shared/components";

@Component({
  selector: "app-expense-receipt-upload",
  templateUrl: "./expense-receipt-upload.html",
  imports: [UiFileUpload, UiFormField, Icon, Stack],
})
export class ExpenseReceiptUpload {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  public readonly initialPath = input<string | null>(null);
  public readonly chooseLabel = input<string>("Upload Receipt");
  public readonly uploaded = output<string>();

  private readonly uploadedPath = signal<string | null>(null);
  protected readonly hasReceipt = computed(() => !!(this.uploadedPath() ?? this.initialPath()));

  /** `ui-file-upload` has already applied the `accept` guard; it never emits an empty array. */
  async onSelect(files: File[]): Promise<void> {
    const file = files[0];
    if (!file) return;

    const result = await this.api.invoke(uploadExpenseReceipt, {
      body: { File: file },
    });

    if (result?.blobPath) {
      this.uploadedPath.set(result.blobPath);
      this.uploaded.emit(result.blobPath);
      this.toast.showSuccess("Receipt file attached successfully.", "Receipt Uploaded");
    } else {
      this.toast.showError("Failed to upload receipt. Please try again.");
    }
  }

  /** A file the accept/size guards turned away; surfaces a toast so the rejection isn't silent. */
  protected onRejected(reason: string): void {
    this.toast.showError(reason);
  }
}
