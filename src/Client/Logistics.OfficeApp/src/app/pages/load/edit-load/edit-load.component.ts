import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {ConfirmationService, MessageService} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ToastModule} from 'primeng/toast';
import {ButtonModule} from 'primeng/button';
import {DropdownModule} from 'primeng/dropdown';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {AppConfig} from '@configs';
import {AuthService} from '@core/auth';
import {CreateLoad, UpdateLoad, EnumType, LoadStatus, LoadStatuses, Truck} from '@core/models';
import {ApiService} from '@core/services';
import {DistanceUtils} from '@shared/utils';
import {
  AddressAutocompleteComponent,
  DirectionsMapComponent,
  RouteChangedEvent,
  SelectedAddressEvent,
} from '@shared/components';


@Component({
  selector: 'app-edit-load',
  templateUrl: './edit-load.component.html',
  styleUrls: ['./edit-load.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    ToastModule,
    ConfirmDialogModule,
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
  providers: [
    ConfirmationService,
  ],
})
export class EditLoadComponent implements OnInit {
  public readonly accessToken: string;
  private distanceMeters: number;

  public id: string | null;
  public headerText: string;
  public isBusy: boolean;
  public editMode: boolean;
  public form: FormGroup;
  public suggestedDrivers: SuggestedDriver[];
  public loadStatuses: EnumType[];
  public originCoords?: [number, number] | null;
  public destinationCoords?: [number, number] | null;

  constructor(
    private authService: AuthService,
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private route: ActivatedRoute)
  {
    this.accessToken = AppConfig.mapboxToken;
    this.isBusy = false;
    this.editMode = true;
    this.headerText = 'Edit a load';
    this.suggestedDrivers = [];
    this.loadStatuses = LoadStatuses;
    this.distanceMeters = 0;
    this.id = null;

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
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    this.fetchCurrentDispatcher();

    if (!this.id) {
      this.editMode = false;
      this.headerText = 'Add a new load';
    }
    else {
      this.fetchLoad(this.id);
      this.headerText = 'Edit a load';
    }
  }

  searchTruck(event: any) {
    this.apiService.getTruckDrivers(event.query).subscribe((result) => {
      if (!result.success || !result.items) {
        return;
      }

      this.suggestedDrivers = result.items.map((truckDriver) => (
        {
          truckId: truckDriver.truck.id,
          driversName: this.formatDriversName(truckDriver.truck),
        }),
      );
    });
  }

  submit() {
    const assignedTruck = this.form.value.assignedTruck;

    if (!assignedTruck) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select a truck'});
      return;
    }

    if (this.editMode) {
      this.updateLoad();
    }
    else {
      this.createLoad();
    }
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
    const distanceMiles = DistanceUtils.metersTo(this.distanceMeters, 'mi');
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
            this.resetForm();
          }

          this.isBusy = false;
        });
  }

  private updateLoad() {
    const command: UpdateLoad = {
      id: this.id!,
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
      status: this.form.value.status,
    };

    this.apiService.updateLoad(command)
        .subscribe((result) => {
          if (result.success) {
            this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'Load has been updated successfully'});
          }

          this.isBusy = false;
        });
  }

  private deleteLoad() {
    if (!this.id) {
      return;
    }

    this.isBusy = true;
    this.apiService.deleteLoad(this.id).subscribe((result) => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'A load has been deleted successfully'});
        this.resetForm();

        this.isBusy = false;
      }
    });
  }

  private fetchCurrentDispatcher() {
    const userData = this.authService.getUserData();
    this.form.patchValue({
      dispatcherName: userData?.name,
      dispatcherId: userData?.id,
    });
  }

  private fetchLoad(id: string) {
    this.apiService.getLoad(id).subscribe((result) => {
      if (result.success && result.value) {
        const load = result.value;

        this.form.patchValue({
          name: load.name,
          orgAddress: load.originAddress,
          dstAddress: load.destinationAddress,
          dispatchedDate: this.getLocaleDate(load.dispatchedDate),
          deliveryCost: load.deliveryCost,
          distance: DistanceUtils.metersTo(load.distance, 'mi'),
          dispatcherName: load.assignedDispatcherName,
          dispatcherId: load.assignedDispatcherId,
          status: load.status,
          assignedTruck: {
            truckId: load.assignedTruck.id,
            driversName: this.formatDriversName(load.assignedTruck)},
        });

        this.originCoords = [load.originAddressLong, load.originAddressLat];
        this.destinationCoords = [load.destinationAddressLong, load.destinationAddressLat];
      }
    });
  }

  private resetForm() {
    this.form.reset();

    this.form.patchValue({
      dispatchedDate: new Date().toLocaleDateString(),
      deliveryCost: 0,
      distance: 0,
      orgAddress: '',
      orgCoords: [],
      dstAddress: '',
      dstCoords: [],
    });

    this.editMode = false;
    this.headerText = 'Add a new load';
    this.originCoords = null;
    this.destinationCoords = null;
    this.id = null;
    this.fetchCurrentDispatcher();
  }

  private formatDriversName(truck: Truck): string {
    const driversName = truck.drivers.map((driver) => driver.fullName);
    return `${truck.truckNumber} - ${driversName}`;
  }

  private getLocaleDate(dateStr?: string | Date): string {
    if (dateStr) {
      return new Date(dateStr).toLocaleDateString();
    }
    return '';
  }
}

interface SuggestedDriver {
  driversName: string,
  truckId: string;
}
