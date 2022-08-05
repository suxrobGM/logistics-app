import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import * as MapboxGeocoder from '@mapbox/mapbox-gl-geocoder';
import * as MapboxDirections from '@mapbox/mapbox-gl-directions/dist/mapbox-gl-directions';
import * as mapboxgl from 'mapbox-gl';
import { MessageService } from 'primeng/api';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Load } from '@shared/models';
import { ApiService } from '@shared/services';

@Component({
  selector: 'app-edit-load',
  templateUrl: './edit-load.component.html',
  styleUrls: ['./edit-load.component.scss']
})
export class EditLoadComponent implements OnInit {
  private accessToken = 'pk.eyJ1Ijoic3V4cm9iZ20iLCJhIjoiY2w0dmsyMGd1MDEzZDNjcXcwZGRtY255MSJ9.XwGTNZx_httMhW0Fu34udQ' // TODO: load access token from config file
  private map!: mapboxgl.Map;
  private directions!: any;
  private load?: Load;

  public id?: string;
  public form: FormGroup;
  public isBusy: boolean;
  public headerText: string;
  public distance: number;
  
  //public hideDestAddressInput = false;

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private oidcSecurityService: OidcSecurityService) 
  {
    this.isBusy = false;
    this.headerText = 'Edit a Load';
    this.distance = 0;

    this.form = new FormGroup({
      'name': new FormControl('')
    });
  }

  public ngOnInit(): void {
    this.id = history.state.id;
    this.initMapbox();
    this.initGeocoderInputs();
  }

  public onSubmit() {

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
      this.distance = this.convertToMiles(data.route[0].distance);
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
      console.log('Source address: ' + data.result.place_name);
      //this.hideDestAddressInput = false;
      this.directions.setOrigin(data.result.center);
      //this.ref.markForCheck();
    });

    // srcGeocoder.on('loading', () => {
    //   this.hideDestAddressInput = true;
    // });

    destGeocoder.on('result', (data: any) => {
      console.log('Destination address ' + data.result.place_name);
      this.directions.setDestination(data.result.center);
    });
  }

  private convertToMiles(meters: number): number {
    const miles = meters*0.000621371;
    return Number.parseFloat(miles.toFixed(2));
  }
}
