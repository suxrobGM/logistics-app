import { Component, type OnInit, effect, inject, input, output, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { DividerModule } from "primeng/divider";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { Select } from "primeng/select";
import { ToastModule } from "primeng/toast";
import {
  type AddressDto,
  type CustomerDto,
  type GeoPointDto,
  type LoadStatus,
  type LoadType,
  type TruckDto,
  loadStatusOptions,
  loadTypeOptions,
} from "@/core/api/models";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import {
  AddressAutocomplete,
  DirectionMap,
  LabeledField,
  type RouteChangeEvent,
  SearchCustomerComponent,
  SearchTruckComponent,
  type SelectedAddressEvent,
  ValidationSummary,
  type Waypoint,
} from "@/shared/components";
import { Converters } from "@/shared/utils";

/**
 * Form value interface for the Load Form.
 */
export interface LoadFormValue {
  name: string;
  type: LoadType;
  customer: CustomerDto;
  originAddress: AddressDto;
  originLocation: GeoPointDto;
  destinationAddress: AddressDto;
  destinationLocation: GeoPointDto;
  deliveryCost: number;
  distance: number; // miles, read-only for users
  status?: LoadStatus | null; // only present in edit-mode
  assignedTruckId: string;
  assignedDispatcherId: string;
  assignedDispatcherName: string;
  tripId?: string | null;
  tripNumber?: number | null;
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
    Select,
    RouterLink,
    AddressAutocomplete,
    DirectionMap,
    ValidationSummary,
    LabeledField,
    SearchCustomerComponent,
    SearchTruckComponent,
    DividerModule,
  ],
})
export class LoadFormComponent implements OnInit {
  protected readonly loadTypes = loadTypeOptions;
  protected readonly loadStatuses = loadStatusOptions;
  private readonly dummyLocation: GeoPointDto = { longitude: 0, latitude: 0 };

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
    customer: new FormControl<CustomerDto | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    originAddress: new FormControl<AddressDto | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    originLocation: new FormControl<GeoPointDto>(this.dummyLocation, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    destinationAddress: new FormControl<AddressDto | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    destinationLocation: new FormControl<GeoPointDto>(this.dummyLocation, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    deliveryCost: new FormControl(0, { validators: [Validators.required], nonNullable: true }),
    distance: new FormControl({ value: 0, disabled: true }, { nonNullable: true }),
    // only visible/patched when mode === 'edit'
    status: new FormControl<LoadStatus | null>(null),
    assignedTruck: new FormControl<TruckDto | string | null>(
      { value: null, disabled: !this.canChangeAssignedTruck() },
      {
        validators: [Validators.required],
        nonNullable: true,
      },
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
    console.log("form errors", this.form.errors);

    if (this.form.invalid) {
      return;
    }

    const formRawValue = this.form.getRawValue();

    const formValue: LoadFormValue = {
      ...formRawValue,
      distance: Converters.toMeters(formRawValue.distance, "mi"),
      assignedTruckId: (formRawValue.assignedTruck as TruckDto).id,
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
