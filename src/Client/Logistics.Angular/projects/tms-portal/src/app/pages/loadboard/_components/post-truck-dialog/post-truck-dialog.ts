import { Component, inject, input, model, output } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import {
  type LoadBoardProviderType,
  type PostTruckToLoadBoardCommand,
  type TruckDto,
} from "@logistics/shared/api";
import { Grid, Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { FormField } from "@/shared/components";
import { EQUIPMENT_OPTIONS } from "../loadboard.constants";

interface ProviderOption {
  label: string;
  value: LoadBoardProviderType;
}

@Component({
  selector: "app-post-truck-dialog",
  templateUrl: "./post-truck-dialog.html",
  imports: [
    ButtonModule,
    DatePickerModule,
    DialogModule,
    FormField,
    Grid,
    InputNumberModule,
    InputTextModule,
    ReactiveFormsModule,
    SelectModule,
    Typography,
  ],
})
export class PostTruckDialog {
  private readonly fb = inject(FormBuilder);

  public readonly visible = model.required<boolean>();
  public readonly posting = input(false);
  public readonly trucks = input.required<TruckDto[]>();
  public readonly providerOptions = input.required<ProviderOption[]>();
  public readonly submitted = output<PostTruckToLoadBoardCommand>();

  protected readonly equipmentOptions = EQUIPMENT_OPTIONS;

  protected readonly form = this.fb.group({
    truckId: ["", Validators.required],
    providerType: ["demo" as LoadBoardProviderType, Validators.required],
    availableAtCity: ["", Validators.required],
    availableAtState: ["", Validators.required],
    availableAtZipCode: [""],
    destinationCity: [""],
    destinationState: [""],
    destinationRadius: [null as number | null],
    availableFrom: [new Date(), Validators.required],
    availableTo: [null as Date | null],
    equipmentType: [""],
    maxWeight: [null as number | null],
    maxLength: [null as number | null],
  });

  protected onShow(): void {
    this.form.reset({
      truckId: "",
      providerType: "demo" as LoadBoardProviderType,
      availableAtCity: "",
      availableAtState: "",
      availableAtZipCode: "",
      destinationCity: "",
      destinationState: "",
      destinationRadius: null,
      availableFrom: new Date(),
      availableTo: null,
      equipmentType: "",
      maxWeight: null,
      maxLength: null,
    });
  }

  protected submit(): void {
    if (this.form.invalid) return;
    const v = this.form.value;
    this.submitted.emit({
      truckId: v.truckId!,
      providerType: v.providerType as LoadBoardProviderType,
      availableAtAddress: {
        line1: "N/A",
        city: v.availableAtCity!,
        state: v.availableAtState!,
        zipCode: v.availableAtZipCode || "00000",
        country: "US",
      },
      availableAtLocation: { latitude: 0, longitude: 0 },
      destinationPreference: v.destinationCity
        ? {
            line1: "N/A",
            city: v.destinationCity,
            state: v.destinationState ?? "",
            zipCode: "00000",
            country: "US",
          }
        : undefined,
      destinationRadius: v.destinationRadius || undefined,
      availableFrom: v.availableFrom!.toISOString(),
      availableTo: v.availableTo?.toISOString(),
      equipmentType: v.equipmentType || undefined,
      maxWeight: v.maxWeight || undefined,
      maxLength: v.maxLength || undefined,
    });
  }

  protected close(): void {
    this.visible.set(false);
  }
}
