import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import * as MapboxGeocoder from '@mapbox/mapbox-gl-geocoder';
import * as MapboxDirections from '@mapbox/mapbox-gl-directions/dist/mapbox-gl-directions';
import * as mapboxgl from 'mapbox-gl';
import { MessageService } from 'primeng/api';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Employee, Load, LoadStatus, UserIdentity } from '@shared/models';
import { ApiService } from '@shared/services';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-edit-load',
  templateUrl: './edit-load.component.html',
  styleUrls: ['./edit-load.component.scss']
})
export class EditLoadComponent implements OnInit {
  private accessToken = 'pk.eyJ1Ijoic3V4cm9iZ20iLCJhIjoiY2w0dmsyMGd1MDEzZDNjcXcwZGRtY255MSJ9.XwGTNZx_httMhW0Fu34udQ' // TODO: load access token from config file
  private map!: mapboxgl.Map;
  private directions!: any;

  public id?: string;
  public headerText: string;
  public isBusy: boolean;
  public editMode: boolean;
  public form: FormGroup;
  public suggestedDrivers: Employee[];
  public loadStatuses: string[]
  
  //public hideDestAddressInput = false;

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService) 
  {
    this.isBusy = false;
    this.editMode = true;
    this.headerText = 'Edit a load';
    this.suggestedDrivers = [];
    this.loadStatuses = [];

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
      status: new FormControl(LoadStatus.Dispatched, Validators.required)
    });
  }

  public ngOnInit(): void {
    this.id = history.state.id;
    this.initMapbox();
    this.initGeocoderInputs();

    for (const loadStatus in LoadStatus) {
      this.loadStatuses.push(loadStatus);
    }

    this.oidcSecurityService.getUserData().subscribe((user: UserIdentity) => {
      this.form.patchValue({
        dispatcherName: user.name,
        dispatcherId: user.sub
      });
    });

    this.form.reset();

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

  public onSubmit() {
    console.log(this.form.value);
    const driver = this.form.value.driver as Employee;

    if (!driver) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'Select a driver'});
      return;
    }

    const load: Load = {
      name: this.form.value.name,
      sourceAddress: this.form.value.srcAddress,
      destinationAddress: this.form.value.dstAddress,
      dispatchedDate: new Date(),
      deliveryCost: this.form.value.deliveryCost,
      distance: this.form.value.distance,
      assignedDispatcherId: this.form.value.dispatcherId,
      assignedDriverId: driver.id,
      status: this.form.status
    }

    this.isBusy = true;
    if (this.editMode) {
      this.apiService.updateLoad(load).subscribe(result => {
        if (result.success) {
          this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'Load has been updated successfully'});
        }

        this.isBusy = false;
      });
    }
    else {
      this.apiService.createLoad(load).subscribe(result => {
        if (result.success) {
          this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'A new load has been created successfully'});
        }

        this.form.reset();
        this.isBusy = false;
      });
    }
  }

  private initMapbox() {
    this.map = new mapboxgl.Map({
      container: 'map',
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
      const distanceMeters = data.route[0].distance;
      const distanceMiles = this.convertToMiles(distanceMeters);
      this.form.patchValue({distance: distanceMiles});
    });

    this.map.addControl(this.directions, 'top-left');
  }

  private initGeocoderInputs() {
    const srcGeocoder = new MapboxGeocoder({
      accessToken: this.accessToken,
      countries: 'us'
    });

    const destGeocoder = new MapboxGeocoder({
      accessToken: this.accessToken,
      countries: 'us',
    });

    srcGeocoder.addTo('#srcAddress');
    destGeocoder.addTo('#dstAddress');

    srcGeocoder.on('result', (data: any) => {
      const address = data.result.place_name;
      //this.hideDestAddressInput = false;
      this.directions.setOrigin(data.result.center);
      this.form.patchValue({srcAddress: address});
      //this.ref.markForCheck();
    });

    // srcGeocoder.on('loading', () => {
    //   this.hideDestAddressInput = true;
    // });

    destGeocoder.on('result', (data: any) => {
      const address = data.result.place_name;
      this.directions.setDestination(data.result.center);
      this.form.patchValue({dstAddress: address});
    });
  }

  private fetchLoad(id: string) {
    this.apiService.getLoad(id).subscribe(result => {
      if (result.success && result.value) {
        const load = result.value;
        console.log(load);
        

        this.form.patchValue({
          name: load.name,
          srcAddress: load.sourceAddress,
          dstAddress: load.destinationAddress,
          dispatchedDate: this.getLocaleDate(load.dispatchedDate),
          deliveryCost: load.deliveryCost,
          dispatcherName: load.assignedDispatcherName,
          dispatcherId: load.assignedDispatcherId,
          status: load.status,
          driver: {
            id: load.assignedDriverId,
            userName: load.assignedDriverName
          }
        });
      }
    });
  }

  private getLocaleDate(dateStr?: string | Date): string {
    if (dateStr) {
      return new Date(dateStr).toLocaleDateString();
    }
    return '';
  }

  private convertToMiles(meters: number): number {
    const miles = meters*0.000621371;
    return Number.parseFloat(miles.toFixed(2));
  }
}
