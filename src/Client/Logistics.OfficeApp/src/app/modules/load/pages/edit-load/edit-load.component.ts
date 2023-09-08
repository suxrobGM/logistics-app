import {Component, ElementRef, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import {ActivatedRoute} from '@angular/router';
import * as MapboxGeocoder from '@mapbox/mapbox-gl-geocoder';
import * as MapboxDirections from '@mapbox/mapbox-gl-directions/dist/mapbox-gl-directions';
import * as mapboxgl from 'mapbox-gl';
import {ConfirmationService, MessageService} from 'primeng/api';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {AppConfig} from '@core';
import {CreateLoad, Employee, Load, TruckDriver, UpdateLoad, UserIdentity} from '@shared/models';
import {ApiService} from '@shared/services';
import {DistanceUnitPipe} from '@shared/pipes';
import {EnumType, LoadStatus, LoadStatuses} from '@shared/types';

@Component({
  selector: 'app-edit-load',
  templateUrl: './edit-load.component.html',
  styleUrls: ['./edit-load.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class EditLoadComponent implements OnInit {
  private accessToken = AppConfig.mapboxToken;
  private map!: mapboxgl.Map;
  private directions!: any;
  private destGeocoder!: MapboxGeocoder;
  private orgGeocoder!: MapboxGeocoder;
  private distanceMeters: number;

  public id?: string;
  public headerText: string;
  public isBusy: boolean;
  public editMode: boolean;
  public form: FormGroup;
  public suggestedDrivers: SuggestedDriver[];
  public loadStatuses: EnumType[];

  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private oidcService: OidcSecurityService,
    private distanceUnit: DistanceUnitPipe,
    private route: ActivatedRoute)
  {
    this.isBusy = false;
    this.editMode = true;
    this.headerText = 'Edit a load';
    this.suggestedDrivers = [];
    this.loadStatuses = LoadStatuses;
    this.distanceMeters = 0;

    this.form = new FormGroup({
      name: new FormControl(''),
      orgAddress: new FormControl('', Validators.required),
      orgCoords: new FormControl('', Validators.required),
      dstAddress: new FormControl('', Validators.required),
      dstCoords: new FormControl('', Validators.required),
      dispatchedDate: new FormControl(new Date().toLocaleDateString(), Validators.required),
      deliveryCost: new FormControl(0, Validators.required),
      distance: new FormControl(0, Validators.required),
      dispatcherName: new FormControl('', Validators.required),
      dispatcherId: new FormControl('', Validators.required),
      assignedTruck: new FormControl('', Validators.required),
      status: new FormControl(LoadStatus.Dispatched, Validators.required),
    });
  }

  public ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params['id'];
    });

    this.initMapbox();
    this.initGeocoderInputs();
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

  public searchTruck(event: any) {
    this.apiService.getTruckDrivers(event.query).subscribe((result) => {
      if (!result.success || !result.items) {
        return;
      }

      this.suggestedDrivers = result.items.map((truckDriver) => {
        const driversName = truckDriver.drivers.map((driver) => driver.fullName);

        return {
          truckId: truckDriver.truck.id,
          driversName: this.formatDriversName(truckDriver.truck.truckNumber, driversName),
        };
      });
    });
  }

  public submit() {
    const assignedTruck = this.form.value.assignedTruck;

    if (!assignedTruck) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select a truck'});
      return;
    }

    // if (this.editMode) {
    //   this.updateLoad();
    // }
    // else {
    //   this.createLoad();
    // }
  }

  public confirmToDelete() {
    this.confirmationService.confirm({
      message: 'Are you sure that you want to delete this load?',
      accept: () => this.deleteLoad(),
    });
  }

  private createLoad() {
    this.isBusy = true;

    const command: CreateLoad = {
      name: this.form.value.name,
      originAddress: this.form.value.orgAddress,
      originCoordinates: this.form.value.orgCoords,
      destinationAddress: this.form.value.dstAddress,
      destinationCoordinates: this.form.value.dstCoords,
      deliveryCost: this.form.value.deliveryCost,
      distance: this.distanceMeters,
      assignedDispatcherId: this.form.value.dispatcherId,
      assignedTruckId: this.form.value.assignedTruckId,
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
      originCoordinates: this.form.value.orgCoords,
      destinationAddress: this.form.value.dstAddress,
      destinationCoordinates: this.form.value.dstCoords,
      deliveryCost: this.form.value.deliveryCost,
      distance: this.distanceMeters,
      assignedDispatcherId: this.form.value.dispatcherId,
      assignedTruckId: this.form.value.assignedTruckId,
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

  private initMapbox() {
    this.map = new mapboxgl.Map({
      container: 'routeMap',
      accessToken: this.accessToken,
      style: 'mapbox://styles/mapbox/streets-v11',
      center: [-74.5, 40],
      zoom: 6,
    });

    this.directions = new MapboxDirections({
      accessToken: this.accessToken,
      profile: 'mapbox/driving-traffic',
      congestion: true,
      interactive: false,
      controls: {
        profileSwitcher: false,
        instructions: false,
        inputs: false,
      },
    });

    this.directions.on('route', (data: any) => {
      this.distanceMeters = data.route[0].distance;
      const distanceMiles = this.toMi(this.distanceMeters);
      this.form.patchValue({distance: distanceMiles});
    });

    this.map.addControl(this.directions, 'top-left');
  }

  private initGeocoderInputs() {
    this.orgGeocoder = new MapboxGeocoder({
      accessToken: this.accessToken,
      countries: 'us',
    });

    this.destGeocoder = new MapboxGeocoder({
      accessToken: this.accessToken,
      countries: 'us',
    });

    this.orgGeocoder.addTo('#orgAddress');
    this.destGeocoder.addTo('#dstAddress');

    this.orgGeocoder.on('result', (data: any) => {
      this.directions.setOrigin(data.result.center);
      this.form.patchValue({
        orgAddress: data.result.place_name,
        orgCoords: data.result.center,
      });
    });

    this.destGeocoder.on('result', (data: any) => {
      this.directions.setDestination(data.result.center);
      this.form.patchValue({
        dstAddress: data.result.place_name,
        dstCoords: data.result.center,
      });
    });
  }

  private fetchCurrentDispatcher() {
    this.oidcService.getUserData().subscribe((user: UserIdentity) => {
      this.form.patchValue({
        dispatcherName: user.name,
        dispatcherId: user.sub,
      });
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
          distance: this.toMi(load.distance),
          dispatcherName: load.assignedDispatcherName,
          dispatcherId: load.assignedDispatcherId,
          status: load.status,
          assignedTruck: {
            truckId: load.assignedTruckId,
            driversName: this.formatDriversName(load.assignedTruckNumber, load.assignedTruckDriversName)},
        });

        this.orgGeocoder.query(load.originAddress!);
        this.destGeocoder.query(load.destinationAddress!);
        this.directions.setOrigin(load.originAddress!);
        this.directions.setDestination(load.destinationAddress!);
      }
    });
  }

  private resetForm() {
    this.form.reset();

    this.form.patchValue({
      dispatchedDate: new Date().toLocaleDateString(),
      deliveryCost: 0,
      distance: 0,
    });

    this.orgGeocoder.clear();
    this.destGeocoder.clear();
    this.directions.removeRoutes();
    this.editMode = false;
    this.headerText = 'Add a new load';
    this.id = undefined;
    this.fetchCurrentDispatcher();
  }

  private formatDriversName(truckNumber: string, driversName: string[]): string {
    const driversFullName = driversName.join(', ');
    return `${truckNumber} - ${driversFullName}`;
  }

  private getLocaleDate(dateStr?: string | Date): string {
    if (dateStr) {
      return new Date(dateStr).toLocaleDateString();
    }
    return '';
  }

  private toMi(value?: number): number {
    return this.distanceUnit.transform(value, 'mi');
  }
}

interface SuggestedDriver {
  driversName: string,
  truckId: string;
}
