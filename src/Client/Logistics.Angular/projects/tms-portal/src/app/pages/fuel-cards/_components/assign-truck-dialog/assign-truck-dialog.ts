import { Component, ElementRef, input, model, output, signal, viewChild } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { type FuelCardTransactionDto, type TruckDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Stack,
  Surface,
  Typography,
  UiButton,
  UiCheckboxField,
  UiDialog,
  UiSelectField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { UiFormField } from "@/shared/components";

export interface AssignTruckRequest {
  transactionId: string;
  truckId: string;
  rememberMapping: boolean;
}

const EMPTY = {
  truckId: null as string | null,
  rememberMapping: true,
};

@Component({
  selector: "app-assign-truck-dialog",
  templateUrl: "./assign-truck-dialog.html",
  imports: [
    CurrencyFormatPipe,
    DateFormatPipe,
    FormField,
    FormRoot,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiCheckboxField,
    UiDialog,
    UiFormField,
    UiSelectField,
    ValidatedForm,
  ],
})
export class AssignTruckDialog {
  public readonly visible = model.required<boolean>();
  public readonly transaction = input<FuelCardTransactionDto | null>(null);
  public readonly trucks = input.required<TruckDto[]>();
  public readonly saving = input(false);
  public readonly save = output<AssignTruckRequest>();

  private readonly formEl = viewChild.required("formEl", { read: ElementRef });

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.truckId, { message: "Truck is required." });
    },
    {
      submission: {
        action: async () => {
          const transaction = this.transaction();
          const v = this.model();
          if (transaction?.id && v.truckId) {
            this.save.emit({
              transactionId: transaction.id,
              truckId: v.truckId,
              rememberMapping: v.rememberMapping,
            });
          }
          return undefined;
        },
      },
    },
  );

  protected onShow(): void {
    this.form().reset({ ...EMPTY });
  }

  protected requestSubmit(): void {
    (this.formEl().nativeElement as HTMLFormElement).requestSubmit();
  }

  protected close(): void {
    this.visible.set(false);
  }
}
