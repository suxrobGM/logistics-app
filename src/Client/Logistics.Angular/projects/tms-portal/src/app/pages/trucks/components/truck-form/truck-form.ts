import { Component, computed, effect, inject, input, output, signal } from "@angular/core";
import { form, FormField, FormRoot, max, min, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import {
  Api,
  getEmployees,
  type AdrEquipmentDto,
  type EmployeeDto,
  type HazmatClass,
  type TruckDto,
  type TruckStatus,
  type TruckType,
} from "@logistics/shared/api";
import { truckStatusOptions, truckTypeOptions } from "@logistics/shared/api/enums";
import {
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiAutocompleteField,
  UiButton,
  UiFormField,
  UiNumberField,
  UiSelectField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { TruckFormTips } from "./truck-form-tips";
import { TruckHazmatSection } from "./truck-hazmat-section";
import { TruckVinField } from "./truck-vin-field";

/** Payload emitted to the parent, which owns the save request. */
export interface TruckFormData {
  truckNumber: string;
  truckType: TruckType;
  truckStatus: TruckStatus;
  mainDriver: EmployeeDto | null;
  secondaryDriver: EmployeeDto | null;
  vehicleCapacity: number | null;
  make: string | null;
  model: string | null;
  year: number | null;
  vin: string | null;
  licensePlate: string | null;
  licensePlateState: string | null;
  adrEquipment: AdrEquipmentDto;
  isHazmatPlacarded: boolean;
}

/**
 * Flat model behind the form. The ADR fields live alongside the rest and are nested only on emit.
 *
 * `make`, `model`, `vin`, `licensePlate`, `licensePlateState`, and `orangePlateNumber` are optional
 * on the wire (`TruckFormData`/DTO) but bind to `ui-text-field`, whose `FormValueControl<string>`
 * is invariant — so they're `string` here (empty string = "no value"), coerced to `null` only when
 * building the outgoing `TruckFormData`.
 */
export interface TruckFormModel {
  truckNumber: string;
  truckType: TruckType;
  truckStatus: TruckStatus;
  mainDriver: EmployeeDto | null;
  secondaryDriver: EmployeeDto | null;
  vehicleCapacity: number | null;
  make: string;
  model: string;
  year: number | null;
  vin: string;
  licensePlate: string;
  licensePlateState: string;
  isAdrCertified: boolean;
  adrCertExpiresAt: Date | null;
  adrAllowedClasses: HazmatClass[];
  orangePlateNumber: string;
  isHazmatPlacarded: boolean;
}

const EMPTY: TruckFormModel = {
  truckNumber: "",
  truckType: "freight_truck",
  truckStatus: "available",
  mainDriver: null,
  secondaryDriver: null,
  vehicleCapacity: null,
  make: "",
  model: "",
  year: null,
  vin: "",
  licensePlate: "",
  licensePlateState: "",
  isAdrCertified: false,
  adrCertExpiresAt: null,
  adrAllowedClasses: [],
  orangePlateNumber: "",
  isHazmatPlacarded: false,
};

@Component({
  selector: "app-truck-form",
  templateUrl: "./truck-form.html",
  imports: [
    FormField,
    FormRoot,
    Grid,
    Icon,
    RouterLink,
    Stack,
    Surface,
    TruckFormTips,
    TruckHazmatSection,
    TruckVinField,
    Typography,
    UiAutocompleteField,
    UiButton,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class TruckForm {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<TruckDto | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<TruckFormData>();
  public readonly remove = output<void>();

  protected readonly truckTypes = truckTypeOptions;
  protected readonly truckStatuses = truckStatusOptions;
  protected readonly suggestedDrivers = signal<EmployeeDto[]>([]);

  protected readonly model = signal<TruckFormModel>({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.truckNumber, { message: "Truck number is required." });
      required(p.truckType, { message: "Truck type is required." });
      required(p.truckStatus, { message: "Status is required." });
      min(p.vehicleCapacity, 1, { message: "Vehicle capacity must be at least 1." });
      max(p.vehicleCapacity, 12, { message: "Vehicle capacity cannot exceed 12." });
      min(p.year, 1900, { message: "Year must be 1900 or later." });
      max(p.year, 2100, { message: "Year cannot be later than 2100." });
    },
    {
      submission: {
        action: async () => {
          const value = this.model();
          this.save.emit({
            truckNumber: value.truckNumber,
            truckType: value.truckType,
            truckStatus: value.truckStatus,
            mainDriver: value.mainDriver,
            secondaryDriver: value.secondaryDriver,
            vehicleCapacity: value.vehicleCapacity,
            // Optional text fields hold "" in the model (ui-text-field is FormValueControl<string>);
            // the wire (TruckFormData) is string | null, same as the old reactive form's `?? null`.
            make: value.make || null,
            model: value.model || null,
            year: value.year,
            vin: value.vin || null,
            licensePlate: value.licensePlate || null,
            licensePlateState: value.licensePlateState || null,
            adrEquipment: {
              isAdrCertified: value.isAdrCertified,
              adrCertExpiresAt: value.adrCertExpiresAt?.toISOString() ?? null,
              allowedClasses: value.adrAllowedClasses,
              orangePlateNumber: value.orangePlateNumber || null,
            },
            isHazmatPlacarded: value.isHazmatPlacarded,
          });
          return undefined;
        },
      },
    },
  );

  protected readonly isCarHauler = computed(() => this.model().truckType === "car_hauler");

  constructor() {
    // Hydrate the model from the async-loaded DTO whenever it arrives.
    effect(() => {
      const initial = this.initial();
      if (initial) {
        this.hydrate(initial);
      }
    });

    // Vehicle capacity only applies to car haulers; clear it for any other type, mirroring the
    // old `truckType.valueChanges` subscription.
    effect(() => {
      if (this.model().truckType !== "car_hauler" && this.model().vehicleCapacity !== null) {
        this.model.update((v) => ({ ...v, vehicleCapacity: null }));
      }
    });
  }

  protected async searchDriver(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getEmployees, {
      Search: event.query,
      Role: "Driver",
    });
    if (result.items) {
      this.suggestedDrivers.set(result.items);
    }
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure you want to delete this truck?",
      accept: () => this.remove.emit(),
    });
  }

  private hydrate(initial: TruckDto): void {
    const adr = initial.adrEquipment;
    this.model.set({
      truckNumber: initial.number ?? "",
      truckType: initial.type ?? "freight_truck",
      truckStatus: initial.status ?? "available",
      mainDriver: initial.mainDriver ?? null,
      secondaryDriver: initial.secondaryDriver ?? null,
      vehicleCapacity: initial.vehicleCapacity ?? null,
      make: initial.make ?? "",
      model: initial.model ?? "",
      year: initial.year ?? null,
      vin: initial.vin ?? "",
      licensePlate: initial.licensePlate ?? "",
      licensePlateState: initial.licensePlateState ?? "",
      isAdrCertified: adr?.isAdrCertified ?? false,
      adrCertExpiresAt: adr?.adrCertExpiresAt ? new Date(adr.adrCertExpiresAt) : null,
      adrAllowedClasses: adr?.allowedClasses ?? [],
      orangePlateNumber: adr?.orangePlateNumber ?? "",
      isHazmatPlacarded: initial.isHazmatPlacarded ?? false,
    });
  }
}
