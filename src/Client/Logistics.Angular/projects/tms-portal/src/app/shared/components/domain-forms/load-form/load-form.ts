import { Component, effect, inject, input, output, signal, type OnInit } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import {
  type Address,
  type ContainerDto,
  type CustomerDto,
  type GeoPoint,
  type LoadSource,
  type LoadStatus,
  type LoadType,
  type TerminalDto,
  type TruckDto,
} from "@logistics/shared/api";
import { loadSourceOptions, loadStatusOptions, loadTypeOptions } from "@logistics/shared/api/enums";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { DatePicker } from "primeng/datepicker";
import { DividerModule } from "primeng/divider";
import { Fieldset } from "primeng/fieldset";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { Select } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastModule } from "primeng/toast";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import {
  AddressAutocomplete,
  DirectionMap,
  type RouteChangeEvent,
  type SelectedAddressEvent,
  type Waypoint,
} from "@/shared/components/maps";
import {
  SearchContainer,
  SearchCustomer,
  SearchTerminal,
  SearchTruck,
} from "@/shared/components/search";
import { Converters } from "@/shared/utils";

/**
 * Form value interface for the Load Form.
 */
export interface LoadFormValue {
  name: string;
  type: LoadType;
  source: LoadSource;
  customer: CustomerDto;
  originAddress: Address;
  originLocation: GeoPoint;
  destinationAddress: Address;
  destinationLocation: GeoPoint;
  deliveryCost: number;
  distance: number; // miles, read-only for users
  status?: LoadStatus | null; // only present in edit-mode
  assignedTruckId?: string | null; // optional - load can be created without truck assignment
  assignedDispatcherId: string;
  assignedDispatcherName: string;
  tripId?: string | null;
  tripNumber?: number | null;
  // Schedule
  requestedPickupDate?: string | null;
  requestedDeliveryDate?: string | null;
  // Intermodal IDs (emitted from submit)
  containerId?: string | null;
  originTerminalId?: string | null;
  destinationTerminalId?: string | null;
  // Intermodal DTOs (for patching from edit-mode initial data)
  container?: ContainerDto | null;
  originTerminal?: TerminalDto | null;
  destinationTerminal?: TerminalDto | null;
  // Notes
  notes?: string | null;
}

@Component({
  selector: "app-load-form",
  templateUrl: "./load-form.html",
  imports: [
    ToastModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    InputTextModule,
    InputGroupModule,
    InputGroupAddonModule,
    InputNumberModule,
    ButtonModule,
    DatePicker,
    Fieldset,
    Select,
    TextareaModule,
    RouterLink,
    AddressAutocomplete,
    DirectionMap,
    ValidationSummary,
    LabeledField,
    SearchContainer,
    SearchCustomer,
    SearchTerminal,
    SearchTruck,
    DividerModule,
  ],
})
export class LoadForm implements OnInit {
  protected readonly loadTypes = loadTypeOptions;
  protected readonly loadStatuses = loadStatusOptions;
  protected readonly loadSources = loadSourceOptions;
  private readonly dummyLocation: GeoPoint = { longitude: 0, latitude: 0 };

  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  protected readonly originCoords = signal<Waypoint>({
    id: "origin",
    location: { longitude: 0, latitude: 0 },
  });
  protected readonly destinationCoords = signal<Waypoint>({
    id: "destination",
    location: { longitude: 0, latitude: 0 },
  });

  public readonly mode = input.required<"create" | "edit">();
  public readonly canChangeAssignedTruck = input<boolean>(true);
  public readonly initial = input<Partial<LoadFormValue> | null>(null);
  public readonly isLoading = input(false);
  public readonly mapHeight = input<string>("100%");

  public readonly save = output<LoadFormValue>();
  public readonly remove = output<void>();

  protected readonly form = new FormGroup({
    name: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    type: new FormControl<LoadType>("general_freight", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    source: new FormControl<LoadSource>("manual", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    customer: new FormControl<CustomerDto | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    originAddress: new FormControl<Address | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    originLocation: new FormControl<GeoPoint>(this.dummyLocation, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    destinationAddress: new FormControl<Address | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    destinationLocation: new FormControl<GeoPoint>(this.dummyLocation, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    deliveryCost: new FormControl(0, { validators: [Validators.required], nonNullable: true }),
    distance: new FormControl({ value: 0, disabled: true }, { nonNullable: true }),
    // Schedule
    requestedPickupDate: new FormControl<Date | null>(null),
    requestedDeliveryDate: new FormControl<Date | null>(null),
    // Intermodal — controls hold the full DTO objects (autocompletes return DTOs);
    // submit() extracts the IDs and emits them on LoadFormValue.
    container: new FormControl<ContainerDto | null>(null),
    originTerminal: new FormControl<TerminalDto | null>(null),
    destinationTerminal: new FormControl<TerminalDto | null>(null),
    notes: new FormControl<string | null>(null, { validators: [Validators.maxLength(2000)] }),
    // only visible/patched when mode === 'edit'
    status: new FormControl<LoadStatus | null>(null),
    // Truck assignment is optional - load can be created without a truck (e.g., from load board)
    assignedTruck: new FormControl<TruckDto | string | null>(
      { value: null, disabled: !this.canChangeAssignedTruck() },
      { nonNullable: true },
    ),
    assignedDispatcherId: new FormControl("", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    assignedDispatcherName: new FormControl({ value: "", disabled: true }, { nonNullable: true }),
    tripId: new FormControl<string | null>({ value: null, disabled: true }),
    tripNumber: new FormControl<number | null>({ value: null, disabled: true }),
  });

  constructor() {
    effect(() => {
      const initialData = this.initial();
      if (!initialData) {
        return;
      }

      // Prevent overwriting user changes if the form is dirty in edit-mode
      if (this.mode() === "edit" && this.form.dirty) {
        return;
      }

      this.patch(initialData);
    });
  }

  ngOnInit(): void {
    if (this.mode() === "create") {
      this.setCurrentDispatcher();
    }
  }

  protected updateOrigin(e: SelectedAddressEvent): void {
    this.originCoords.set({
      id: "origin",
      location: {
        longitude: e.center[0],
        latitude: e.center[1],
      },
    });
    this.form.patchValue({ originLocation: { longitude: e.center[0], latitude: e.center[1] } });
  }

  protected updateDestination(e: SelectedAddressEvent): void {
    this.destinationCoords.set({
      id: "destination",
      location: {
        longitude: e.center[0],
        latitude: e.center[1],
      },
    });
    this.form.patchValue({
      destinationLocation: { longitude: e.center[0], latitude: e.center[1] },
    });
  }

  protected updateDistance(e: RouteChangeEvent): void {
    const miles = Converters.metersTo(e.distance, "mi");
    this.form.patchValue({ distance: miles });
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }

    const raw = this.form.getRawValue();
    const truck = raw.assignedTruck as TruckDto | null;

    const formValue: LoadFormValue = {
      ...raw,
      distance: Converters.toMeters(raw.distance, "mi"),
      assignedTruckId: truck?.id ?? null,
      requestedPickupDate: raw.requestedPickupDate
        ? new Date(raw.requestedPickupDate).toISOString()
        : null,
      requestedDeliveryDate: raw.requestedDeliveryDate
        ? new Date(raw.requestedDeliveryDate).toISOString()
        : null,
      containerId: raw.container?.id ?? null,
      originTerminalId: raw.originTerminal?.id ?? null,
      destinationTerminalId: raw.destinationTerminal?.id ?? null,
    } as LoadFormValue;

    this.save.emit(formValue);
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this load?",
      accept: () => this.remove.emit(),
    });
  }

  private patch(src: Partial<LoadFormValue>): void {
    this.form.patchValue({
      ...src,
      assignedTruck: src.assignedTruckId, // Set ID instead of object, then component will fetch the object
      requestedPickupDate: src.requestedPickupDate ? new Date(src.requestedPickupDate) : null,
      requestedDeliveryDate: src.requestedDeliveryDate ? new Date(src.requestedDeliveryDate) : null,
      container: src.container ?? null,
      originTerminal: src.originTerminal ?? null,
      destinationTerminal: src.destinationTerminal ?? null,
    });

    if (src.originLocation) {
      this.originCoords.set({
        id: "origin",
        location: {
          longitude: src.originLocation.longitude,
          latitude: src.originLocation.latitude,
        },
      });
    }
    if (src.destinationLocation) {
      this.destinationCoords.set({
        id: "destination",
        location: {
          longitude: src.destinationLocation.longitude,
          latitude: src.destinationLocation.latitude,
        },
      });
    }
  }

  private setCurrentDispatcher(): void {
    const userData = this.authService.getUserData();

    if (userData) {
      this.form.patchValue({
        assignedDispatcherId: userData.id,
        assignedDispatcherName: userData.getFullName(),
      });
    }
  }
}
