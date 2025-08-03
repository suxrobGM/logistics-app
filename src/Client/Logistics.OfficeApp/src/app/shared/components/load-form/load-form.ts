import {Component, OnInit, effect, inject, input, output, signal} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {RouterLink} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {InputGroupModule} from "primeng/inputgroup";
import {InputGroupAddonModule} from "primeng/inputgroupaddon";
import {InputNumberModule} from "primeng/inputnumber";
import {InputTextModule} from "primeng/inputtext";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {Select} from "primeng/select";
import {ToastModule} from "primeng/toast";
import {
  AddressDto,
  CustomerDto,
  GeoPointDto,
  LoadStatus,
  LoadType,
  loadStatusOptions,
  loadTypeOptions,
} from "@/core/api/models";
import {AuthService} from "@/core/auth";
import {
  AddressAutocomplete,
  DirectionMap,
  FormField,
  RouteChangedEvent,
  SearchCustomerComponent,
  SearchTruckComponent,
  SelectedAddressEvent,
  ValidationSummary,
} from "@/shared/components";
import {GeoPoint} from "@/shared/types/mapbox";
import {Converters} from "@/shared/utils";

/**
 * Form value interface for the Load Form.
 */
export interface LoadFormValue {
  name: string;
  loadType: LoadType;
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
    FormField,
    SearchCustomerComponent,
    SearchTruckComponent,
  ],
})
export class LoadFormComponent implements OnInit {
  protected readonly loadTypes = loadTypeOptions;
  protected readonly loadStatuses = loadStatusOptions;
  private readonly dummyLocation: GeoPointDto = {longitude: 0, latitude: 0};

  private readonly authService = inject(AuthService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<LoadFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<LoadFormValue>();
  public readonly remove = output<void>();

  protected readonly originCoords = signal<GeoPoint>([0, 0]);
  protected readonly destinationCoords = signal<GeoPoint>([0, 0]);

  protected readonly form = new FormGroup({
    name: new FormControl("", {validators: [Validators.required], nonNullable: true}),
    loadType: new FormControl(LoadType.GeneralFreight, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    customer: new FormControl<CustomerDto | null>(null, {validators: [Validators.required]}),
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
    deliveryCost: new FormControl(0, {validators: [Validators.required], nonNullable: true}),
    distance: new FormControl({value: 0, disabled: true}, {nonNullable: true}),
    // only visible/patched when mode === 'edit'
    status: new FormControl<LoadStatus | null>(null),
    assignedTruckId: new FormControl("", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    assignedDispatcherId: new FormControl("", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    assignedDispatcherName: new FormControl({value: "", disabled: true}, {nonNullable: true}),
    tripId: new FormControl<string | null>({value: null, disabled: true}),
    tripNumber: new FormControl<number | null>({value: null, disabled: true}),
  });

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  ngOnInit(): void {
    if (this.mode() === "create") {
      this.setCurrentDispatcher();
    }
  }

  protected updateOrigin(e: SelectedAddressEvent): void {
    this.originCoords.set(e.center);
    this.form.patchValue({originLocation: {longitude: e.center[0], latitude: e.center[1]}});
  }

  protected updateDestination(e: SelectedAddressEvent): void {
    this.destinationCoords.set(e.center);
    this.form.patchValue({destinationLocation: {longitude: e.center[0], latitude: e.center[1]}});
  }

  protected updateDistance(e: RouteChangedEvent): void {
    const miles = Converters.metersTo(e.distance, "mi");
    this.form.patchValue({distance: miles});
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.save.emit(this.form.getRawValue() as LoadFormValue);
  }

  protected askRemove(): void {
    this.remove.emit();
  }

  private patch(src: Partial<LoadFormValue>): void {
    this.form.patchValue({
      ...src,
    });

    if (src.originLocation) {
      this.originCoords.set([src.originLocation.longitude, src.originLocation.latitude]);
    }
    if (src.destinationLocation) {
      this.destinationCoords.set([
        src.destinationLocation.longitude,
        src.destinationLocation.latitude,
      ]);
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
