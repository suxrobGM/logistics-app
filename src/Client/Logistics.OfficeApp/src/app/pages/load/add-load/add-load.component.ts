import {Component, OnInit} from "@angular/core";
import {NgIf} from "@angular/common";
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from "@angular/forms";
import {Router, RouterLink} from "@angular/router";
import {CardModule} from "primeng/card";
import {ToastModule} from "primeng/toast";
import {ButtonModule} from "primeng/button";
import {DropdownModule} from "primeng/dropdown";
import {AutoCompleteModule} from "primeng/autocomplete";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {GLOBAL_CONFIG} from "@/configs";
import {AuthService} from "@/core/auth";
import {AddressDto, CreateLoadCommand, CustomerDto} from "@/core/models";
import {ApiService, ToastService} from "@/core/services";
import {Converters} from "@/core/utils";
import {
  AddressAutocompleteComponent,
  DirectionsMapComponent,
  RouteChangedEvent,
  SelectedAddressEvent,
  ValidationSummaryComponent,
} from "@/components";
import {SearchCustomerComponent, SearchTruckComponent} from "../components";
import {TruckData} from "../shared";

@Component({
  selector: "app-add-load",
  templateUrl: "./add-load.component.html",
  styleUrls: ["./add-load.component.scss"],
  standalone: true,
  imports: [
    ToastModule,
    CardModule,
    NgIf,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    DropdownModule,
    ButtonModule,
    RouterLink,
    AddressAutocompleteComponent,
    DirectionsMapComponent,
    SearchCustomerComponent,
    SearchTruckComponent,
    ValidationSummaryComponent,
  ],
})
export class AddLoadComponent implements OnInit {
  public readonly accessToken = GLOBAL_CONFIG.mapboxToken;
  private distanceMeters = 0;
  public isLoading = false;
  public form: FormGroup<AddLoadForm>;
  public originCoords: [number, number] | null = null;
  public destinationCoords: [number, number] | null = null;

  constructor(
    private readonly authService: AuthService,
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly router: Router
  ) {
    this.form = new FormGroup<AddLoadForm>({
      name: new FormControl(""),
      customer: new FormControl(null, {validators: Validators.required}),
      orgAddress: new FormControl(null, {validators: Validators.required, nonNullable: true}),
      orgCoords: new FormControl([0, 0], {validators: Validators.required, nonNullable: true}),
      dstAddress: new FormControl(null, {validators: Validators.required, nonNullable: true}),
      dstCoords: new FormControl([0, 0], {validators: Validators.required, nonNullable: true}),
      deliveryCost: new FormControl(0, {validators: Validators.required, nonNullable: true}),
      distance: new FormControl({value: 0, disabled: true}, {validators: Validators.required, nonNullable: true}),
      assignedTruck: new FormControl(null, {validators: Validators.required}),
      assignedDispatcherId: new FormControl("", {validators: Validators.required, nonNullable: true}),
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
    this.originCoords = eventData.center;
    this.form.patchValue({
      orgCoords: eventData.center,
    });
  }

  updateDestination(eventData: SelectedAddressEvent) {
    this.destinationCoords = eventData.center;
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

    this.isLoading = true;
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

      this.isLoading = false;
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
