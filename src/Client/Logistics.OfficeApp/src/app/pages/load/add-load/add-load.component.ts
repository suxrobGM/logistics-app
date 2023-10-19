/* eslint-disable @typescript-eslint/no-non-null-assertion */
import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {Router, RouterLink} from '@angular/router';
import {CardModule} from 'primeng/card';
import {ToastModule} from 'primeng/toast';
import {ButtonModule} from 'primeng/button';
import {DropdownModule} from 'primeng/dropdown';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {AppConfig} from '@configs';
import {AuthService} from '@core/auth';;
import {CreateLoad, Customer} from '@core/models';
import {ApiService, ToastService} from '@core/services';
import {DistanceConverter} from '@shared/utils';
import {
  AddressAutocompleteComponent,
  DirectionsMapComponent,
  RouteChangedEvent,
  SelectedAddressEvent,
} from '@shared/components';
import {SearchCustomerComponent, SearchTruckComponent, TruckData} from '../components';


@Component({
  selector: 'app-add-load',
  templateUrl: './add-load.component.html',
  styleUrls: ['./add-load.component.scss'],
  encapsulation: ViewEncapsulation.None,
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
  ],
})
export class AddLoadComponent implements OnInit {
  public readonly accessToken: string;
  private distanceMeters: number;

  public isLoading: boolean;
  public form: FormGroup;
  public selectedTruck: TruckData | null;
  public selectedCustomer: Customer | null;
  public suggestedCustomers: Customer[];
  public originCoords?: [number, number] | null;
  public destinationCoords?: [number, number] | null;

  constructor(
    private readonly authService: AuthService,
    private readonly apiService: ApiService,
    private readonly toastService: ToastService,
    private readonly router: Router)
  {
    this.accessToken = AppConfig.mapboxToken;
    this.isLoading = false;
    this.selectedTruck = null;
    this.selectedCustomer = null;
    this.suggestedCustomers = [];
    this.distanceMeters = 0;

    this.form = new FormGroup({
      name: new FormControl(''),
      orgAddress: new FormControl('', Validators.required),
      orgCoords: new FormControl([], Validators.required),
      dstAddress: new FormControl('', Validators.required),
      dstCoords: new FormControl([], Validators.required),
      dispatchedDate: new FormControl(new Date().toLocaleDateString(), Validators.required),
      deliveryCost: new FormControl(0, Validators.required),
      distance: new FormControl(0, Validators.required),
      dispatcherName: new FormControl('', Validators.required),
      dispatcherId: new FormControl('', Validators.required),
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
    const distanceMiles = DistanceConverter.metersTo(this.distanceMeters, 'mi');
    this.form.patchValue({distance: distanceMiles});
  }

  createLoad() {
    if (!this.isValid()) {
      return;
    }

    this.isLoading = true;
    const command: CreateLoad = {
      name: this.form.value.name,
      originAddress: this.form.value.orgAddress,
      originAddressLong: this.form.value.orgCoords[0],
      originAddressLat: this.form.value.orgCoords[1],
      destinationAddress: this.form.value.dstAddress,
      destinationAddressLong: this.form.value.dstCoords[0],
      destinationAddressLat: this.form.value.dstCoords[1],
      deliveryCost: this.form.value.deliveryCost,
      distance: this.distanceMeters,
      assignedDispatcherId: this.form.value.dispatcherId,
      assignedTruckId: this.selectedTruck!.truckId,
      customerId: this.selectedCustomer!.id
    };
    
    this.apiService.createLoad(command)
      .subscribe((result) => {
        if (result.isSuccess) {
          this.toastService.showSuccess('A new load has been created successfully');
          this.router.navigateByUrl('/loads');
        }

        this.isLoading = false;
      });
  }

  private isValid(): boolean {
    if (!this.selectedTruck) {
      this.toastService.showError('Please select a truck');
      return false;
    }

    if (!this.selectedCustomer) {
      this.toastService.showError('Please select a customer');
      return false;
    }

    if (!this.form.value.orgAddress) {
      this.toastService.showError('Please select the origin address');
      return false;
    }

    if (!this.form.value.dstAddress) {
      this.toastService.showError('Please select the destination address');
      return false;
    }

    return true;
  }

  private fetchCurrentDispatcher() {
    const userData = this.authService.getUserData();
    this.form.patchValue({
      dispatcherName: userData?.name,
      dispatcherId: userData?.id,
    });
  }
}
