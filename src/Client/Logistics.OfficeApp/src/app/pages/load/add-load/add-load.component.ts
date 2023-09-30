import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {Router, RouterLink} from '@angular/router';
import {MessageService} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {ToastModule} from 'primeng/toast';
import {ButtonModule} from 'primeng/button';
import {DropdownModule} from 'primeng/dropdown';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {AppConfig} from '@configs';
import {AuthService} from '@core/auth';
import {CreateLoad, EnumType, LoadStatus, LoadStatuses, Truck} from '@core/models';
import {ApiService} from '@core/services';
import {DistanceConverter} from '@shared/utils';
import {
  AddressAutocompleteComponent,
  DirectionsMapComponent,
  RouteChangedEvent,
  SelectedAddressEvent,
} from '@shared/components';
import {TruckData, TruckHelper} from '../shared';


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
  ],
})
export class AddLoadComponent implements OnInit {
  public readonly accessToken: string;
  private distanceMeters: number;

  public isBusy: boolean;
  public form: FormGroup;
  public suggestedDrivers: TruckData[];
  public loadStatuses: EnumType[];
  public originCoords?: [number, number] | null;
  public destinationCoords?: [number, number] | null;

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private messageService: MessageService,
    private router: Router)
  {
    this.accessToken = AppConfig.mapboxToken;
    this.isBusy = false;
    this.suggestedDrivers = [];
    this.loadStatuses = LoadStatuses;
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
      assignedTruck: new FormControl('', Validators.required),
      status: new FormControl(LoadStatus.Dispatched, Validators.required),
    });
  }

  ngOnInit(): void {
    this.fetchCurrentDispatcher();
  }

  searchTruck(event: any) {
    TruckHelper.findTruckDrivers(this.apiService, event.query).subscribe((drivers) => this.suggestedDrivers = drivers);
  }

  submit() {
    const assignedTruck = this.form.value.assignedTruck;

    if (!assignedTruck) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select a truck'});
      return;
    }

    this.createLoad();
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

  private createLoad() {
    this.isBusy = true;

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
      assignedTruckId: this.form.value.assignedTruck.truckId,
    };

    this.apiService.createLoad(command)
        .subscribe((result) => {
          if (result.success) {
            this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'A new load has been created successfully'});
            this.router.navigateByUrl('/load/list');
          }

          this.isBusy = false;
        });
  }

  private fetchCurrentDispatcher() {
    const userData = this.authService.getUserData();
    this.form.patchValue({
      dispatcherName: userData?.name,
      dispatcherId: userData?.id,
    });
  }
}
