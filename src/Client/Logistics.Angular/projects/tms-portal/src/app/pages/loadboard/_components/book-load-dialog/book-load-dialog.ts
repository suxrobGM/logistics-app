import { Component, inject, input, model, output } from "@angular/core";
import { FormBuilder, ReactiveFormsModule } from "@angular/forms";
import {
  type LoadBoardBookingRequest,
  type LoadBoardListingDto,
  type TruckDto,
} from "@logistics/shared/api";
import { Alert, Stack, Surface, Typography } from "@logistics/shared/components";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { FormField } from "@/shared/components";

@Component({
  selector: "app-book-load-dialog",
  templateUrl: "./book-load-dialog.html",
  imports: [
    Alert,
    ButtonModule,
    CurrencyFormatPipe,
    DialogModule,
    FormField,
    InputTextModule,
    ReactiveFormsModule,
    SelectModule,
    Stack,
    Surface,
    Typography,
  ],
})
export class BookLoadDialog {
  private readonly fb = inject(FormBuilder);

  public readonly visible = model.required<boolean>();
  public readonly listing = input<LoadBoardListingDto | null>(null);
  public readonly booking = input(false);
  public readonly trucks = input.required<TruckDto[]>();
  public readonly submitted = output<LoadBoardBookingRequest>();

  protected readonly form = this.fb.group({
    truckId: [""],
    contactName: [""],
    contactPhone: [""],
    contactEmail: [""],
    notes: [""],
  });

  protected onShow(): void {
    this.form.reset({
      truckId: "",
      contactName: "",
      contactPhone: "",
      contactEmail: "",
      notes: "",
    });
  }

  protected submit(): void {
    const v = this.form.value;
    this.submitted.emit({
      truckId: v.truckId || undefined,
      notes: v.notes,
    });
  }

  protected close(): void {
    this.visible.set(false);
  }
}
