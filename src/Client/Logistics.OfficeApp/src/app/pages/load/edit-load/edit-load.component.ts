/* eslint-disable @typescript-eslint/no-non-null-assertion */
import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {ConfirmationService} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ToastModule} from 'primeng/toast';
import {ButtonModule} from 'primeng/button';
import {DropdownModule} from 'primeng/dropdown';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {AppConfig} from '@configs';
import {EnumType, LoadStatus, LoadStatusEnum} from '@core/enums';
import {Address, Customer, UpdateLoad} from '@core/models';
import {ApiService, ToastService} from '@core/services';
import {Converters} from '@shared/utils';
import {
  AddressAutocompleteComponent,
  DirectionsMapComponent,
  RouteChangedEvent,
  SelectedAddressEvent,
  ValidationSummaryComponent,
} from '@shared/components';
import {TruckData, TruckHelper} from '../shared';
import {SearchCustomerComponent, SearchTruckComponent} from '../components';



@Component({
  selector: 'app-edit-load',
  templateUrl: './edit-load.component.html',
  styleUrls: ['./edit-load.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ToastModule,
    ConfirmDialogModule,
    CardModule,
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
  providers: [
    ConfirmationService,
  ],
})
export class EditLoadComponent implements OnInit {
  public readonly accessToken = AppConfig.mapboxToken;
  private distanceMeters = 0;
  public id!: string;
  public loadRefId!: number;
  public isLoading = false;
  public form: FormGroup<EditLoadForm>;
  public loadStatuses = LoadStatusEnum.toArray();
  public originCoords: [number, number] | null = null;
  public destinationCoords: [number, number] | null = null;

  constructor(
    private readonly apiService: ApiService,
    private readonly confirmationService: ConfirmationService,
    private readonly toastService: ToastService,
    private readonly route: ActivatedRoute,
    private readonly router: Router)
  {
    this.form = new FormGroup<EditLoadForm>({
      name: new FormControl(''),
      customer: new FormControl(null, {validators: Validators.required}),
      orgAddress: new FormControl(null, {validators: Validators.required, nonNullable: true}),
      orgCoords: new FormControl([0,0], {validators: Validators.required, nonNullable: true}),
      dstAddress: new FormControl(null, {validators: Validators.required, nonNullable: true}),
      dstCoords: new FormControl([0,0], {validators: Validators.required, nonNullable: true}),
      deliveryCost: new FormControl(0, {validators: Validators.required, nonNullable: true}),
      distance: new FormControl({value: 0, disabled: true}, {validators: Validators.required, nonNullable: true}),
      status: new FormControl(LoadStatusEnum.getValue(LoadStatus.Dispatched), {validators: Validators.required, nonNullable: true}),
      assignedTruck: new FormControl(null, {validators: Validators.required}),
      assignedDispatcherId: new FormControl('', {validators: Validators.required, nonNullable: true}),
      assignedDispatcherName: new FormControl({value: '', disabled: true}, {validators: Validators.required, nonNullable: true}),
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    if (!this.id) {
      this.toastService.showError('Missing the reqiured id parameter');
      return;
    }

    this.fetchLoad();
  }

  confirmToDelete() {
    this.confirmationService.confirm({
      message: 'Are you sure that you want to delete this load?',
      accept: () => this.deleteLoad(),
    });
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
    const distanceMiles = Converters.metersTo(this.distanceMeters, 'mi');
    this.form.patchValue({distance: distanceMiles});
  }

  updateLoad() {
    if (!this.form.valid) {
      return;
    }

    this.isLoading = true;
    const command: UpdateLoad = {
      id: this.id,
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
      status: this.form.value.status?.value as LoadStatus,
    };

    this.apiService.updateLoad(command)
      .subscribe((result) => {
        if (result.isSuccess) {
          this.toastService.showSuccess('Load has been updated successfully');
        }

        this.isLoading = false;
      });
  }

  private deleteLoad() {
    this.isLoading = true;
    this.apiService.deleteLoad(this.id).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('A load has been deleted successfully');
        this.router.navigateByUrl('/loads');
      }

      this.isLoading = false;
    });
  }

  private fetchLoad() {
    this.isLoading = true;

    this.apiService.getLoad(this.id).subscribe((result) => {
      if (result.isError || !result.data) {
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
        distance: Converters.metersTo(load.distance, 'mi'),
        status: LoadStatusEnum.getValue(load.status),
        assignedDispatcherId: load.assignedDispatcherId,
        assignedDispatcherName: load.assignedDispatcherName,
        assignedTruck: {
          truckId: load.assignedTruckId!,
          driversName: TruckHelper.formatDriversName(load.assignedTruckNumber!, load.assignedTruckDriversName!)},
      });

      this.loadRefId = load.refId;
      this.originCoords = [load.originAddressLong, load.originAddressLat];
      this.destinationCoords = [load.destinationAddressLong, load.destinationAddressLat];
      this.isLoading = false;
    });
  }

  private getLocaleDate(dateStr?: string | Date): string {
    if (dateStr) {
      return new Date(dateStr).toLocaleDateString();
    }
    return '';
  }
}

interface EditLoadForm {
  name: FormControl<string | null>;
  customer: FormControl<Customer | null>;
  orgAddress: FormControl<Address | null>;
  orgCoords: FormControl<[number, number]>;
  dstAddress: FormControl<Address | null>;
  dstCoords: FormControl<[number, number]>;
  deliveryCost: FormControl<number>;
  distance: FormControl<number>;
  status: FormControl<EnumType>;
  assignedTruck: FormControl<TruckData | null>;
  assignedDispatcherId: FormControl<string>;
  assignedDispatcherName: FormControl<string>;
}
