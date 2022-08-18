import { Component, OnInit } from '@angular/core';
import { Load } from '@shared/models';
import { ApiService } from '@shared/services';
import * as mapboxgl from 'mapbox-gl';

@Component({
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss']
})
export class DashboardPageComponent implements OnInit {
  private map!: mapboxgl.Map;
  public loads: Load[];

  constructor(private apiService: ApiService) {
    this.loads = [];
  }

  ngOnInit() {
    this.map = new mapboxgl.Map({
      container: 'map',
      accessToken: 'pk.eyJ1Ijoic3V4cm9iZ20iLCJhIjoiY2w0dmsyMGd1MDEzZDNjcXcwZGRtY255MSJ9.XwGTNZx_httMhW0Fu34udQ',
      style: 'mapbox://styles/mapbox/streets-v11',
      center: [-74.5, 40],
      zoom: 6
    });

    this.apiService.getLoads().subscribe(result => {
      if (result.success && result.items) {
        this.loads = result.items;
      }
    });
  }
}
