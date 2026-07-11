import { Component, inject, input, model, output, signal } from "@angular/core";
import { UiFormField } from "@logistics/shared";
import { Api, reportLoadException, type LoadExceptionType } from "@logistics/shared/api";
import { loadExceptionTypeOptions } from "@logistics/shared/api/enums";
import { UiButton, UiDialog, UiSelectField, UiTextareaField } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-report-exception-dialog",
  templateUrl: "./report-exception-dialog.html",
  imports: [UiButton, UiDialog, UiFormField, UiSelectField, UiTextareaField],
})
export class ReportExceptionDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly loadId = input.required<string>();
  public readonly visible = model<boolean>(false);
  public readonly reported = output<void>();

  protected readonly selectedType = signal<LoadExceptionType | null>(null);
  protected readonly reason = signal<string>("");
  protected readonly isSubmitting = signal(false);

  protected readonly typeOptions = loadExceptionTypeOptions;

  async submit(): Promise<void> {
    const type = this.selectedType();
    const reasonText = this.reason().trim();

    if (!type) {
      this.toastService.showWarning("Please select an exception type");
      return;
    }

    if (!reasonText) {
      this.toastService.showWarning("Please provide a reason for the exception");
      return;
    }

    this.isSubmitting.set(true);
    try {
      await this.api.invoke(reportLoadException, {
        id: this.loadId(),
        body: { type, reason: reasonText },
      });
      this.toastService.showSuccess("Exception reported successfully");
      this.reported.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to report exception");
    } finally {
      this.isSubmitting.set(false);
    }
  }

  close(): void {
    this.selectedType.set(null);
    this.reason.set("");
    this.visible.set(false);
  }
}
