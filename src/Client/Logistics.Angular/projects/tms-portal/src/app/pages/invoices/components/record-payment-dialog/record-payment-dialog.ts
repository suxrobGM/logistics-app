import { Component, inject, input, model, output, signal } from "@angular/core";
import { form, FormField, FormRoot, min, required } from "@angular/forms/signals";
import { UiFormField } from "@logistics/shared";
import { Api, recordManualPayment, type PaymentMethodType } from "@logistics/shared/api";
import { paymentMethodTypeOptions } from "@logistics/shared/api/enums";
import {
  Stack,
  UiButton,
  UiDateField,
  UiDialog,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";

const EMPTY = {
  amount: null as number | null,
  type: "cash" as PaymentMethodType,
  referenceNumber: "",
  notes: "",
  receivedDate: null as Date | null,
};

@Component({
  selector: "app-record-payment-dialog",
  templateUrl: "./record-payment-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Stack,
    UiButton,
    UiDateField,
    UiDialog,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
  ],
})
export class RecordPaymentDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly invoiceId = input.required<string>();
  public readonly outstandingAmount = input<number>(0);
  public readonly visible = model<boolean>(false);
  public readonly recorded = output<void>();

  protected readonly paymentTypes = paymentMethodTypeOptions;

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.amount, { message: "Amount is required." });
      min(p.amount, 0.01, { message: "Amount must be greater than zero." });
      required(p.type, { message: "Payment type is required." });
    },
    {
      submission: {
        action: async () => {
          const receivedDate = this.model().receivedDate;
          try {
            await this.api.invoke(recordManualPayment, {
              id: this.invoiceId(),
              body: {
                amount: this.model().amount!,
                type: this.model().type,
                referenceNumber: this.model().referenceNumber || undefined,
                notes: this.model().notes || undefined,
                receivedDate: receivedDate ? receivedDate.toISOString() : undefined,
              },
            });
          } catch {
            this.toastService.showError("Failed to record payment");
            return undefined;
          }
          this.toastService.showSuccess("Payment recorded successfully");
          this.recorded.emit();
          this.close();
          return undefined;
        },
      },
    },
  );

  onShow(): void {
    this.form().reset({ ...EMPTY, amount: this.outstandingAmount(), receivedDate: new Date() });
  }

  close(): void {
    this.visible.set(false);
    this.form().reset({ ...EMPTY });
  }
}
