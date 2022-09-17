import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import * as MapboxGeocoder from '@mapbox/mapbox-gl-geocoder';
import * as MapboxDirections from '@mapbox/mapbox-gl-directions/dist/mapbox-gl-directions';
import * as mapboxgl from 'mapbox-gl';
import { ConfirmationService, MessageService } from 'primeng/api';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Employee, EnumType, Load, LoadStatuses, UserIdentity } from '@shared/models';
import { ApiService } from '@shared/services';
import { AppConfig } from '@configs';
import { DistanceUnitPipe } from '@shared/pipes';

@Component({
  selector: 'app-edit-load',
  templateUrl: './edit-load.component.html',
  styleUrls: ['./edit-load.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditLoadComponent implements OnInit {
  private accessToken = AppConfig.mapboxToken;
  private map!: mapboxgl.Map;
  private directions!: any;
  private destGeocoder!: MapboxGeocoder;
  private srcGeocoder!: MapboxGeocoder;
  private distanceMeters: number;

  public id?: string;
  public headerText: string;
  public isBusy: boolean;
  public editMode: boolean;
  public form: FormGroup;
  public suggestedDrivers: Employee[];
  public loadStatuses: EnumType[]

  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService,
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
      srcAddress: new FormControl('', Validators.required),
      dstAddress: new FormControl('', Validators.required),
      dispatchedDate: new FormControl(new Date().toLocaleDateString(), Validators.required),
      deliveryCost: new FormControl(0, Validators.required),
      distance: new FormControl(0, Validators.required),
      dispatcherName: new FormControl('', Validators.required),
      dispatcherId: new FormControl('', Validators.required),
      driver: new FormControl('', Validators.required),
      status: new FormControl(LoadStatuses[0], Validators.required)
    });
  }

  public ngOnInit(): void {
    this.route.params.subscribe(params => {
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

  public searchDriver(event: any) {
    this.apiService.getDrivers(event.query).subscribe(result => {
      if (result.success && result.items) {
        this.suggestedDrivers = result.items;
      }
    });
  }

  public submit() {
    const driver = this.form.value.driver as Employee;

    if (!driver) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select a driver'});
      return;
    }

    const load: Load = {
      name: this.form.value.name,
      sourceAddress: this.form.value.srcAddress,
      destinationAddress: this.form.value.dstAddress,
      deliveryCost: this.form.value.deliveryCost,
      distance: this.distanceMeters,
      assignedDispatcherId: this.form.value.dispatcherId,
      assignedDriverId: driver.id,
      status: this.form.value.status
    }

    if (this.id) {
      load.id = this.id;
    }

    this.isBusy = true;
    if (this.editMode) {
      this.apiService.updateLoad(load)
        .subscribe(result => {
          if (result.success) {
            this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'Load has been updated successfully'});
          }

          this.isBusy = false;
      });
    }
    else {
      this.apiService.createLoad(load)
        .subscribe(result => {
          if (result.success) {
            this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'A new load has been created successfully'});
            this.resetForm();
          }

          this.isBusy = false;
      });
    }
  }

  public confirmToDelete() {
    this.confirmationService.confirm({
      message: 'Are you sure that you want to delete this load?',
      accept: () => this.deleteLoad()
    });
  }

  private deleteLoad() {
    if (!this.id) {
      return;
    }

    this.isBusy = true;
    this.apiService.deleteLoad(this.id).subscribe(result => {
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
      zoom: 6
    });

    this.directions = new MapboxDirections({
      accessToken: this.accessToken,
      profile: 'mapbox/driving-traffic',
      congestion: true,
      interactive: false,
      controls: {
        profileSwitcher: false,
        instructions: false,
        inputs: false
      }
    });

    this.directions.on('route', (data: any) => {
      this.distanceMeters = data.route[0].distance;
      const distanceMiles = this.getMiles(this.distanceMeters);
      this.form.patchValue({distance: distanceMiles});
    });

    this.map.addControl(this.directions, 'top-left');
  }

  private initGeocoderInputs() {
    this.srcGeocoder = new MapboxGeocoder({
      accessToken: this.accessToken,
      countries: 'us'
    });

    this.destGeocoder = new MapboxGeocoder({
      accessToken: this.accessToken,
      countries: 'us',
    });

    this.srcGeocoder.addTo('#srcAddress');
    this.destGeocoder.addTo('#dstAddress');

    this.srcGeocoder.on('result', (data: any) => {
      const address = data.result.place_name;
      this.directions.setOrigin(data.result.center);
      this.form.patchValue({srcAddress: address});
    });

    this.destGeocoder.on('result', (data: any) => {
      const address = data.result.place_name;
      this.directions.setDestination(data.result.center);
      this.form.patchValue({dstAddress: address});
    });
  }

  private fetchCurrentDispatcher() {
    this.oidcSecurityService.getUserData().subscribe((user: UserIdentity) => {
      this.form.patchValue({
        dispatcherName: user.name,
        dispatcherId: user.sub
      });
    });
  }

  private fetchLoad(id: string) {
    this.apiService.getLoad(id).subscribe(result => {
      if (result.success && result.value) {
        const load = result.value;

        this.form.patchValue({
          name: load.name,
          srcAddress: load.sourceAddress,
          dstAddress: load.destinationAddress,
          dispatchedDate: this.getLocaleDate(load.dispatchedDate),
          deliveryCost: load.deliveryCost,
          distance: this.getMiles(load.distance),
          dispatcherName: load.assignedDispatcherName,
          dispatcherId: load.assignedDispatcherId,
          status: load.status,
          driver: {
            id: load.assignedDriverId,
            userName: load.assignedDriverName
          }
        });
        
        this.srcGeocoder.query(load.sourceAddress!);
        this.destGeocoder.query(load.destinationAddress!);
        this.directions.setOrigin(load.sourceAddress!);
        this.directions.setDestination(load.destinationAddress!);
      }
    });
  }

  private resetForm() {
    this.form.reset();

    this.form.patchValue({
      dispatchedDate: new Date().toLocaleDateString(),
      deliveryCost: 0,
      distance: 0
    });

    this.srcGeocoder.clear();
    this.destGeocoder.clear();
    this.directions.removeRoutes();
    this.editMode = false;
    this.headerText = 'Add a new load';
    this.id = undefined;
    this.fetchCurrentDispatcher();
  }

  private getLocaleDate(dateStr?: string | Date): string {
    if (dateStr) {
      return new Date(dateStr).toLocaleDateString();
    }
    return '';
  }

  private getMiles(value?: number): number {
    return this.distanceUnit.transform(value, 'mi');
  }
}
