import { HttpClient } from "@angular/common/http";
import { Component, inject, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LabeledField } from "@logistics/shared";
import { ApiConfiguration } from "@logistics/shared/api";
import { type LoadExceptionType } from "@logistics/shared/api";
import { loadExceptionTypeOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { firstValueFrom } from "rxjs";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-report-exception-dialog",
  templateUrl: "./report-exception-dialog.html",
  imports: [
    DialogModule,
    ButtonModule,
    FormsModule,
    SelectModule,
    TextareaModule,
    LabeledField,
  ],
})
export class ReportExceptionDialog {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);
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
      const url = `${this.apiConfig.rootUrl}/loads/${this.loadId()}/exceptions`;
      await firstValueFrom(
        this.http.post(url, {
          type,
          reason: reasonText,
        }),
      );
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
