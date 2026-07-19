import { Component, ElementRef, input, model, output, signal, viewChild } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { type TruckDto } from "@logistics/shared/api";
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
  truckId: string;
  rememberMapping: boolean;
}

const EMPTY = {
  truckId: null as string | null,
  rememberMapping: true,
};

/**
 * Truck picker used for both a single transaction and a bulk selection. The caller owns which
 * transaction(s) are being assigned; this dialog only collects the truck + remember-mapping choice
 * and emits it. `summary` describes what's being assigned; `suggestedTruckId` pre-selects a match.
 */
@Component({
  selector: "app-assign-truck-dialog",
  templateUrl: "./assign-truck-dialog.html",
  imports: [
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
  public readonly trucks = input.required<TruckDto[]>();
  public readonly saving = input(false);
  /** What's being assigned - a single transaction's detail line, or "12 transactions selected". */
  public readonly summary = input<string>("");
  /** Truck to pre-select on open (e.g. matched by unit number). Null leaves the field empty. */
  public readonly suggestedTruckId = input<string | null>(null);
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
          const v = this.model();
          if (v.truckId) {
            this.save.emit({ truckId: v.truckId, rememberMapping: v.rememberMapping });
          }
          return undefined;
        },
      },
    },
  );

  protected onShow(): void {
    this.form().reset({ ...EMPTY, truckId: this.suggestedTruckId() });
  }

  protected requestSubmit(): void {
    (this.formEl().nativeElement as HTMLFormElement).requestSubmit();
  }

  protected close(): void {
    this.visible.set(false);
  }
}
