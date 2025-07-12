import {Component, OnInit, inject, signal} from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {Router, RouterLink} from "@angular/router";
import {AutoCompleteModule} from "primeng/autocomplete";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ToastModule} from "primeng/toast";
import {ApiService} from "@/core/api";
import {AddressDto, CreateLoadCommand, CustomerDto} from "@/core/api/models";
import {AuthService} from "@/core/auth";
import {ToastService} from "@/core/services";
import {environment} from "@/env";
import {
  AddressAutocomplete,
  DirectionsMap,
  RouteChangedEvent,
  SelectedAddressEvent,
  ValidationSummary,
} from "@/shared/components";
import {Converters} from "@/shared/utils";
import {SearchCustomerComponent, SearchTruckComponent} from "../components";
import {TruckData} from "../shared";

@Component({
  selector: "app-add-load",
  templateUrl: "./add-load.html",
  styleUrl: "./add-load.css",
  imports: [
    ToastModule,
    CardModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    ButtonModule,
    RouterLink,
    AddressAutocomplete,
    DirectionsMap,
    SearchCustomerComponent,
    SearchTruckComponent,
    ValidationSummary,
  ],
})
export class AddLoadComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  private distanceMeters = 0;
  protected readonly accessToken = environment.mapboxToken;
  protected readonly form: FormGroup<AddLoadForm>;

  protected readonly isLoading = signal(false);
  protected readonly originCoords = signal<[number, number] | null>(null);
  protected readonly destinationCoords = signal<[number, number] | null>(null);

  constructor() {
    this.form = new FormGroup<AddLoadForm>({
      name: new FormControl(""),
      customer: new FormControl(null, {validators: Validators.required}),
      orgAddress: new FormControl(null, {validators: Validators.required, nonNullable: true}),
      orgCoords: new FormControl([0, 0], {validators: Validators.required, nonNullable: true}),
      dstAddress: new FormControl(null, {validators: Validators.required, nonNullable: true}),
      dstCoords: new FormControl([0, 0], {validators: Validators.required, nonNullable: true}),
      deliveryCost: new FormControl(0, {validators: Validators.required, nonNullable: true}),
      distance: new FormControl(
        {value: 0, disabled: true},
        {validators: Validators.required, nonNullable: true}
      ),
      assignedTruck: new FormControl(null, {validators: Validators.required}),
      assignedDispatcherId: new FormControl("", {
        validators: Validators.required,
        nonNullable: true,
      }),
      assignedDispatcherName: new FormControl(
        {value: "", disabled: true},
        {validators: Validators.required, nonNullable: true}
      ),
    });
  }

  ngOnInit(): void {
    this.fetchCurrentDispatcher();
  }

  updateOrigin(eventData: SelectedAddressEvent) {
    this.originCoords.set(eventData.center);
    this.form.patchValue({
      orgCoords: eventData.center,
    });
  }

  updateDestination(eventData: SelectedAddressEvent) {
    this.destinationCoords.set(eventData.center);
    this.form.patchValue({
      dstCoords: eventData.center,
    });
  }

  updateDistance(eventData: RouteChangedEvent) {
    this.distanceMeters = eventData.distance;
    const distanceMiles = Converters.metersTo(this.distanceMeters, "mi");
    this.form.patchValue({distance: distanceMiles});
  }

  createLoad() {
    if (!this.form.valid) {
      return;
    }

    this.isLoading.set(true);
    const command: CreateLoadCommand = {
      name: this.form.value.name!,
      originAddress: this.form.value.orgAddress!,
      originAddressLong: this.form.value.orgCoords![0],
      originAddressLat: this.form.value.orgCoords![1],
      destinationAddress: this.form.value.dstAddress!,
      destinationAddressLong: this.form.value.dstCoords![0],
      destinationAddressLat: this.form.value.dstCoords![1],
      deliveryCost: this.form.value.deliveryCost!,
      distance: this.distanceMeters,
      assignedDispatcherId: this.form.value.assignedDispatcherId!,
      assignedTruckId: this.form.value.assignedTruck!.truckId,
      customerId: this.form.value.customer!.id,
    };

    this.apiService.createLoad(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new load has been created successfully");
        this.router.navigateByUrl("/loads");
      }

      this.isLoading.set(false);
    });
  }

  private fetchCurrentDispatcher() {
    const userData = this.authService.getUserData();

    if (userData) {
      this.form.patchValue({
        assignedDispatcherId: userData.id,
        assignedDispatcherName: userData.getFullName(),
      });
    }
  }
}

interface AddLoadForm {
  name: FormControl<string | null>;
  customer: FormControl<CustomerDto | null>;
  orgAddress: FormControl<AddressDto | null>;
  orgCoords: FormControl<[number, number]>;
  dstAddress: FormControl<AddressDto | null>;
  dstCoords: FormControl<[number, number]>;
  deliveryCost: FormControl<number>;
  distance: FormControl<number>;
  assignedTruck: FormControl<TruckData | null>;
  assignedDispatcherId: FormControl<string>;
  assignedDispatcherName: FormControl<string>;
}
