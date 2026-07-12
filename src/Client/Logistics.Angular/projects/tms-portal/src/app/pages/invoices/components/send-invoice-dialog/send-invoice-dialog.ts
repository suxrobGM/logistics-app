import { Component, inject, input, model, output, signal } from "@angular/core";
import { email, form, FormField, FormRoot, required } from "@angular/forms/signals";
import { UiFormField } from "@logistics/shared";
import { Api, sendInvoice } from "@logistics/shared/api";
import {
  Stack,
  UiButton,
  UiDialog,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";

const EMPTY = { email: "", personalMessage: "" };

@Component({
  selector: "app-send-invoice-dialog",
  templateUrl: "./send-invoice-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Stack,
    UiButton,
    UiDialog,
    UiFormField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class SendInvoiceDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly invoiceId = input.required<string>();
  public readonly customerEmail = input<string | null>();
  public readonly visible = model<boolean>(false);
  public readonly sent = output<void>();

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.email, { message: "Email address is required." });
      email(p.email, { message: "Enter a valid email address." });
    },
    {
      submission: {
        action: async () => {
          try {
            await this.api.invoke(sendInvoice, {
              id: this.invoiceId(),
              body: {
                email: this.model().email,
                personalMessage: this.model().personalMessage || undefined,
              },
            });
          } catch {
            this.toastService.showError("Failed to send invoice");
            return undefined;
          }
          this.toastService.showSuccess("Invoice sent successfully");
          this.sent.emit();
          this.close();
          return undefined;
        },
      },
    },
  );

  onShow(): void {
    this.form().reset({ ...EMPTY, email: this.customerEmail() ?? "" });
  }

  close(): void {
    this.visible.set(false);
    this.form().reset({ ...EMPTY });
  }
}
