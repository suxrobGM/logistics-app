import { Component, input, model, output, signal } from "@angular/core";
import { form, FormField, FormRoot } from "@angular/forms/signals";
import {
  type LoadBoardBookingRequest,
  type LoadBoardListingDto,
  type TruckDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Alert,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiDialog,
  UiSelectField,
  UiTextField,
} from "@logistics/shared/ui";
import { UiFormField } from "@/shared/components";

const EMPTY = {
  truckId: "",
  contactName: "",
  contactPhone: "",
  contactEmail: "",
  notes: "",
};

@Component({
  selector: "app-book-load-dialog",
  templateUrl: "./book-load-dialog.html",
  imports: [
    Alert,
    CurrencyFormatPipe,
    FormField,
    FormRoot,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiDialog,
    UiFormField,
    UiSelectField,
    UiTextField,
  ],
})
export class BookLoadDialog {
  public readonly visible = model.required<boolean>();
  public readonly listing = input<LoadBoardListingDto | null>(null);
  public readonly booking = input(false);
  public readonly trucks = input.required<TruckDto[]>();
  public readonly submitted = output<LoadBoardBookingRequest>();

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(this.model);

  protected onShow(): void {
    this.form().reset({ ...EMPTY });
  }

  protected submit(): void {
    const v = this.model();
    this.submitted.emit({
      truckId: v.truckId || undefined,
      notes: v.notes,
    });
  }

  protected close(): void {
    this.visible.set(false);
  }
}
