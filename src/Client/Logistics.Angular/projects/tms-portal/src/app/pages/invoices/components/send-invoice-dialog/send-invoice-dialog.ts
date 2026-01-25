import { Component, inject, input, model, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { LabeledField } from "@logistics/shared";
import { Api, sendInvoice } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-send-invoice-dialog",
  templateUrl: "./send-invoice-dialog.html",
  imports: [DialogModule, ButtonModule, ReactiveFormsModule, InputTextModule, TextareaModule, LabeledField],
})
export class SendInvoiceDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly invoiceId = input.required<string>();
  public readonly customerEmail = input<string | null>();
  public readonly visible = model<boolean>(false);
  public readonly sent = output<void>();

  protected readonly isSending = signal(false);

  protected readonly form = new FormGroup({
    email: new FormControl("", [Validators.required, Validators.email]),
    personalMessage: new FormControl(""),
  });

  onShow(): void {
    const email = this.customerEmail();
    this.form.reset();
    if (email) {
      this.form.patchValue({ email });
    }
  }

  close(): void {
    this.visible.set(false);
    this.form.reset();
  }

  async send(): Promise<void> {
    if (this.form.invalid) return;

    this.isSending.set(true);
    try {
      await this.api.invoke(sendInvoice, {
        id: this.invoiceId(),
        body: {
          email: this.form.value.email!,
          personalMessage: this.form.value.personalMessage || undefined,
        },
      });
      this.toastService.showSuccess("Invoice sent successfully");
      this.sent.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to send invoice");
    } finally {
      this.isSending.set(false);
    }
  }
}
