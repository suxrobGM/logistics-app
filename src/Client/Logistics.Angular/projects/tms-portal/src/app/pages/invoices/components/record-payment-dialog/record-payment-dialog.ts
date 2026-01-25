import { Component, inject, input, model, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { LabeledField } from "@logistics/shared";
import { Api, recordManualPayment } from "@logistics/shared/api";
import type { PaymentMethodType } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";

interface PaymentTypeOption {
  label: string;
  value: PaymentMethodType;
}

@Component({
  selector: "app-record-payment-dialog",
  templateUrl: "./record-payment-dialog.html",
  imports: [
    DialogModule,
    ButtonModule,
    ReactiveFormsModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    SelectModule,
    DatePickerModule,
    LabeledField,
  ],
})
export class RecordPaymentDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly invoiceId = input.required<string>();
  public readonly outstandingAmount = input<number>(0);
  public readonly visible = model<boolean>(false);
  public readonly recorded = output<void>();

  protected readonly isSaving = signal(false);

  protected readonly paymentTypes: PaymentTypeOption[] = [
    { label: "Cash", value: "cash" },
    { label: "Check", value: "check" },
  ];

  protected readonly form = new FormGroup({
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(0.01)]),
    type: new FormControl<PaymentMethodType>("cash", [Validators.required]),
    referenceNumber: new FormControl(""),
    notes: new FormControl(""),
    receivedDate: new FormControl<Date | null>(null),
  });

  onShow(): void {
    this.form.reset();
    this.form.patchValue({
      amount: this.outstandingAmount(),
      type: "cash",
      receivedDate: new Date(),
    });
  }

  close(): void {
    this.visible.set(false);
    this.form.reset();
  }

  async save(): Promise<void> {
    if (this.form.invalid) return;

    this.isSaving.set(true);
    try {
      const receivedDate = this.form.value.receivedDate;
      await this.api.invoke(recordManualPayment, {
        id: this.invoiceId(),
        body: {
          amount: this.form.value.amount!,
          type: this.form.value.type!,
          referenceNumber: this.form.value.referenceNumber || undefined,
          notes: this.form.value.notes || undefined,
          receivedDate: receivedDate ? receivedDate.toISOString() : undefined,
        },
      });
      this.toastService.showSuccess("Payment recorded successfully");
      this.recorded.emit();
      this.close();
    } catch {
      this.toastService.showError("Failed to record payment");
    } finally {
      this.isSaving.set(false);
    }
  }
}
