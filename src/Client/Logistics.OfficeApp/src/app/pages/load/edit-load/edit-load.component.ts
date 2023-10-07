import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
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
import {UpdateLoad, EnumType, LoadStatus, LoadStatuses, Truck} from '@core/models';
import {ApiService, ToastService} from '@core/services';
import {DistanceConverter} from '@shared/utils';
import {
  AddressAutocompleteComponent,
  DirectionsMapComponent,
  RouteChangedEvent,
  SelectedAddressEvent,
} from '@shared/components';
import {TruckData, TruckHelper} from '../shared';


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

  public id!: string;
  public loadRefId!: number;
  public isBusy: boolean;
  public form: FormGroup;
  public suggestedDrivers: TruckData[];
  public loadStatuses: EnumType[];
  public originCoords?: [number, number] | null;
  public destinationCoords?: [number, number] | null;

  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private toastService: ToastService,
    private route: ActivatedRoute,
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
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    if (!this.id) {
      this.toastService.showError('Missing the reqiured id parameter');
      return;
    }

    this.fetchLoad();
  }

  searchTruck(event: any) {
    TruckHelper.findTruckDrivers(this.apiService, event.query).subscribe((drivers) => this.suggestedDrivers = drivers);
  }

  submit() {
    const assignedTruck = this.form.value.assignedTruck;

    if (!assignedTruck) {
      this.toastService.showError('Select a truck');
      return;
    }

    this.updateLoad();
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
    const distanceMiles = DistanceConverter.metersTo(this.distanceMeters, 'mi');
    this.form.patchValue({distance: distanceMiles});
  }

  private updateLoad() {
    const command: UpdateLoad = {
      id: this.id,
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
          if (result.isSuccess) {
            this.toastService.showSuccess('Load has been updated successfully');
          }

          this.isBusy = false;
        });
  }

  private deleteLoad() {
    this.isBusy = true;
    this.apiService.deleteLoad(this.id).subscribe((result) => {
      if (result.isSuccess) {
        this.toastService.showSuccess('A load has been deleted successfully');
        this.router.navigateByUrl('/load/list');
      }

      this.isBusy = false;
    });
  }

  private fetchLoad() {
    this.apiService.getLoad(this.id).subscribe((result) => {
      if (result.isError) {
        return;
      }

      const load = result.data!;

      this.form.patchValue({
        name: load.name,
        orgAddress: load.originAddress,
        orgCoords: [load.originAddressLong, load.originAddressLat],
        dstAddress: load.destinationAddress,
        dstCoords: [load.destinationAddressLong, load.destinationAddressLat],
        dispatchedDate: this.getLocaleDate(load.dispatchedDate),
        deliveryCost: load.deliveryCost,
        distance: DistanceConverter.metersTo(load.distance, 'mi'),
        dispatcherName: load.assignedDispatcherName,
        dispatcherId: load.assignedDispatcherId,
        status: load.status,
        assignedTruck: {
          truckId: load.assignedTruckId,
          driversName: TruckHelper.formatDriversName(load.assignedTruckNumber!, load.assignedTruckDriversName!)},
      });

      this.loadRefId = load.refId;
      this.originCoords = [load.originAddressLong, load.originAddressLat];
      this.destinationCoords = [load.destinationAddressLong, load.destinationAddressLat];
    });
  }

  private getLocaleDate(dateStr?: string | Date): string {
    if (dateStr) {
      return new Date(dateStr).toLocaleDateString();
    }
    return '';
  }
}
