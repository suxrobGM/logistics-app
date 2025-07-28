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
import {Converters} from "@/shared/utils";

/**
 * Form value interface for the Load Form.
 */
export interface LoadFormValue {
  name: string;
  loadType: LoadType;
  customer: CustomerDto;
  orgAddress: AddressDto;
  orgCoords: [number, number];
  dstAddress: AddressDto;
  dstCoords: [number, number];
  deliveryCost: number;
  distance: number; // miles, read-only for users
  status?: LoadStatus; // only present in edit-mode
  assignedTruckId: string;
  assignedDispatcherId: string;
  assignedDispatcherName: string;
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

  private readonly authService = inject(AuthService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<LoadFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<LoadFormValue>();
  public readonly remove = output<void>();

  protected readonly originCoords = signal<[number, number]>([0, 0]);
  protected readonly destinationCoords = signal<[number, number]>([0, 0]);

  protected readonly form = new FormGroup({
    name: new FormControl("", {validators: [Validators.required], nonNullable: true}),
    loadType: new FormControl(LoadType.GeneralFreight, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    customer: new FormControl<CustomerDto | null>(null, {validators: [Validators.required]}),
    orgAddress: new FormControl<AddressDto | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    orgCoords: new FormControl<[number, number]>([0, 0], {
      validators: [Validators.required],
      nonNullable: true,
    }),
    dstAddress: new FormControl<AddressDto | null>(null, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    dstCoords: new FormControl<[number, number]>([0, 0], {
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

  updateOrigin(e: SelectedAddressEvent) {
    this.originCoords.set(e.center);
    this.form.patchValue({orgCoords: e.center});
  }

  updateDestination(e: SelectedAddressEvent) {
    this.destinationCoords.set(e.center);
    this.form.patchValue({dstCoords: e.center});
  }

  updateDistance(e: RouteChangedEvent) {
    const miles = Converters.metersTo(e.distance, "mi");
    this.form.patchValue({distance: miles});
  }

  submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.save.emit(this.form.getRawValue() as LoadFormValue);
  }

  askRemove(): void {
    this.remove.emit();
  }

  private patch(src: Partial<LoadFormValue>) {
    this.form.patchValue({
      ...src,
    });

    if (src.orgCoords) {
      this.originCoords.set(src.orgCoords);
    }
    if (src.dstCoords) {
      this.destinationCoords.set(src.dstCoords);
    }
  }

  private setCurrentDispatcher() {
    const userData = this.authService.getUserData();

    if (userData) {
      this.form.patchValue({
        assignedDispatcherId: userData.id,
        assignedDispatcherName: userData.getFullName(),
      });
    }
  }
}
