import { Component, input, model, output, signal } from "@angular/core";
import { form, FormField, FormRoot, max, min, required, submit } from "@angular/forms/signals";
import {
  type LoadBoardProviderType,
  type PostTruckToLoadBoardCommand,
  type TruckDto,
} from "@logistics/shared/api";
import {
  Grid,
  Typography,
  UiButton,
  UiDateField,
  UiDialog,
  UiNumberField,
  UiSelectField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { UiFormField } from "@/shared/components";
import { EQUIPMENT_OPTIONS } from "../loadboard.constants";

interface ProviderOption {
  label: string;
  value: LoadBoardProviderType;
}

interface PostTruckFormValue {
  truckId: string;
  providerType: LoadBoardProviderType;
  availableAtCity: string;
  availableAtState: string;
  availableAtZipCode: string;
  destinationCity: string;
  destinationState: string;
  destinationRadius: number | null;
  availableFrom: Date;
  availableTo: Date | null;
  equipmentType: string;
  maxWeight: number | null;
  maxLength: number | null;
}

// A factory (not a frozen const) so `availableFrom` is the current time on each open/reset.
const createEmptyForm = (): PostTruckFormValue => ({
  truckId: "",
  providerType: "demo",
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

@Component({
  selector: "app-post-truck-dialog",
  templateUrl: "./post-truck-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Grid,
    Typography,
    UiButton,
    UiDateField,
    UiDialog,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class PostTruckDialog {
  public readonly visible = model.required<boolean>();
  public readonly posting = input(false);
  public readonly trucks = input.required<TruckDto[]>();
  public readonly providerOptions = input.required<ProviderOption[]>();
  public readonly submitted = output<PostTruckToLoadBoardCommand>();

  protected readonly equipmentOptions = EQUIPMENT_OPTIONS;

  protected readonly model = signal<PostTruckFormValue>(createEmptyForm());

  protected readonly form = form(this.model, (p) => {
    required(p.truckId, { message: "Select a truck." });
    required(p.providerType, { message: "Select a load board." });
    required(p.availableAtCity, { message: "City is required." });
    required(p.availableAtState, { message: "State is required." });
    required(p.availableFrom, { message: "Available from date is required." });
    min(p.destinationRadius, 0, { message: "Radius cannot be negative." });
    max(p.destinationRadius, 500, { message: "Radius cannot exceed 500." });
    min(p.maxWeight, 0, { message: "Max weight cannot be negative." });
    max(p.maxWeight, 100000, { message: "Max weight cannot exceed 100,000 lbs." });
    min(p.maxLength, 0, { message: "Max length cannot be negative." });
    max(p.maxLength, 100, { message: "Max length cannot exceed 100 ft." });
  });

  protected onShow(): void {
    this.form().reset(createEmptyForm());
  }

  /**
   * The dialog's footer button lives outside the `<form>`, so submission is driven imperatively via
   * `submit()` rather than `[formRoot]`.
   */
  protected async onSubmit(): Promise<void> {
    await submit(this.form, async () => {
      const v = this.model();
      this.submitted.emit({
        truckId: v.truckId,
        providerType: v.providerType,
        availableAtAddress: {
          line1: "N/A",
          city: v.availableAtCity,
          state: v.availableAtState,
          zipCode: v.availableAtZipCode || "00000",
          country: "US",
        },
        availableAtLocation: { latitude: 0, longitude: 0 },
        destinationPreference: v.destinationCity
          ? {
              line1: "N/A",
              city: v.destinationCity,
              state: v.destinationState,
              zipCode: "00000",
              country: "US",
            }
          : undefined,
        destinationRadius: v.destinationRadius || undefined,
        availableFrom: v.availableFrom.toISOString(),
        availableTo: v.availableTo?.toISOString(),
        equipmentType: v.equipmentType || undefined,
        maxWeight: v.maxWeight || undefined,
        maxLength: v.maxLength || undefined,
      });
      return undefined;
    });
  }

  protected close(): void {
    this.visible.set(false);
  }
}
