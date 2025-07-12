import {CommonModule} from "@angular/common";
import {Component, inject, input, signal} from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {Router, RouterLink} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {AutoCompleteModule} from "primeng/autocomplete";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {SelectModule} from "primeng/select";
import {ToastModule} from "primeng/toast";
import {ApiService} from "@/core/api";
import {
  AddressDto,
  CustomerDto,
  LoadStatus,
  UpdateLoadCommand,
  loadStatusOptions,
} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {
  AddressAutocomplete,
  DirectionsMap,
  RouteChangedEvent,
  SelectedAddressEvent,
  ValidationSummary,
} from "@/shared/components";
import {Converters} from "@/shared/utils";
import {SearchCustomerComponent, SearchTruckComponent} from "../components";
import {TruckData, TruckHelper} from "../shared";

@Component({
  selector: "app-edit-load",
  templateUrl: "./edit-load.html",
  styleUrl: "./edit-load.css",
  imports: [
    CommonModule,
    ToastModule,
    ConfirmDialogModule,
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
    SelectModule,
  ],
})
export class EditLoadComponent {
  private readonly apiService = inject(ApiService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  private distanceMeters = 0;
  protected readonly loadStatuses = loadStatusOptions;
  protected readonly form: FormGroup<EditLoadForm>;
  protected readonly id = input.required<string>();

  protected readonly loadNumber = signal<number>(0);
  protected readonly isLoading = signal(false);
  protected readonly originCoords = signal<[number, number] | null>(null);
  protected readonly destinationCoords = signal<[number, number] | null>(null);

  constructor() {
    this.form = new FormGroup<EditLoadForm>({
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
      status: new FormControl(LoadStatus.Dispatched, {
        validators: Validators.required,
        nonNullable: true,
      }),
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

    this.fetchLoad();
  }

  confirmToDelete(): void {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this load?",
      accept: () => this.deleteLoad(),
    });
  }

  updateOrigin(eventData: SelectedAddressEvent): void {
    this.originCoords.set(eventData.center);
    this.form.patchValue({
      orgCoords: eventData.center,
    });
  }

  updateDestination(eventData: SelectedAddressEvent): void {
    this.destinationCoords.set(eventData.center);
    this.form.patchValue({
      dstCoords: eventData.center,
    });
  }

  updateDistance(eventData: RouteChangedEvent): void {
    this.distanceMeters = eventData.distance;
    const distanceMiles = Converters.metersTo(this.distanceMeters, "mi");
    this.form.patchValue({distance: distanceMiles});
  }

  updateLoad(): void {
    if (!this.form.valid) {
      return;
    }

    this.isLoading.set(true);
    const command: UpdateLoadCommand = {
      id: this.id(),
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
      customerId: this.form.value.customer?.id,
      status: this.form.value.status,
    };

    this.apiService.updateLoad(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Load has been updated successfully");
      }

      this.isLoading.set(false);
    });
  }

  private deleteLoad(): void {
    this.isLoading.set(true);
    this.apiService.deleteLoad(this.id()).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A load has been deleted successfully");
        this.router.navigateByUrl("/loads");
      }

      this.isLoading.set(false);
    });
  }

  private fetchLoad(): void {
    this.isLoading.set(true);

    this.apiService.getLoad(this.id()).subscribe((result) => {
      if (!result.success || !result.data) {
        return;
      }

      const load = result.data;

      this.form.patchValue({
        name: load.name,
        customer: load.customer,
        orgAddress: load.originAddress,
        orgCoords: [load.originAddressLong, load.originAddressLat],
        dstAddress: load.destinationAddress,
        dstCoords: [load.destinationAddressLong, load.destinationAddressLat],
        deliveryCost: load.deliveryCost,
        distance: Converters.metersTo(load.distance, "mi"),
        status: load.status,
        assignedDispatcherId: load.assignedDispatcherId,
        assignedDispatcherName: load.assignedDispatcherName,
        assignedTruck: {
          truckId: load.assignedTruckId!,
          driversName: TruckHelper.formatDriversName(
            load.assignedTruckNumber!,
            load.assignedTruckDriversName!
          ),
        },
      });

      this.loadNumber.set(load.number);
      this.originCoords.set([load.originAddressLong, load.originAddressLat]);
      this.destinationCoords.set([load.destinationAddressLong, load.destinationAddressLat]);
      this.isLoading.set(false);
    });
  }
}

interface EditLoadForm {
  name: FormControl<string | null>;
  customer: FormControl<CustomerDto | null>;
  orgAddress: FormControl<AddressDto | null>;
  orgCoords: FormControl<[number, number]>;
  dstAddress: FormControl<AddressDto | null>;
  dstCoords: FormControl<[number, number]>;
  deliveryCost: FormControl<number>;
  distance: FormControl<number>;
  status: FormControl<LoadStatus>;
  assignedTruck: FormControl<TruckData | null>;
  assignedDispatcherId: FormControl<string>;
  assignedDispatcherName: FormControl<string>;
}
