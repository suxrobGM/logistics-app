import { Component, computed, inject, input, output, signal } from "@angular/core";
import { Api, uploadExpenseReceipt } from "@logistics/shared/api";
import { Icon, Stack } from "@logistics/shared/components";
import { ToastService } from "@logistics/shared/services";
import { FileUploadModule } from "primeng/fileupload";
import { FormField } from "@/shared/components";

@Component({
  selector: "app-expense-receipt-upload",
  templateUrl: "./expense-receipt-upload.html",
  imports: [FileUploadModule, FormField, Icon, Stack],
})
export class ExpenseReceiptUpload {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  public readonly initialPath = input<string | null>(null);
  public readonly chooseLabel = input<string>("Upload Receipt");
  public readonly uploaded = output<string>();

  private readonly uploadedPath = signal<string | null>(null);
  protected readonly hasReceipt = computed(() => !!(this.uploadedPath() ?? this.initialPath()));

  async onSelect(event: { files: File[] }): Promise<void> {
    if (event.files.length === 0) return;

    const result = await this.api.invoke(uploadExpenseReceipt, {
      body: { File: event.files[0] },
    });

    if (result?.blobPath) {
      this.uploadedPath.set(result.blobPath);
      this.uploaded.emit(result.blobPath);
      this.toast.showSuccess("Receipt file attached successfully.", "Receipt Uploaded");
    } else {
      this.toast.showError("Failed to upload receipt. Please try again.");
    }
  }
}
