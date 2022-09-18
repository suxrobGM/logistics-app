import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { AppConfig } from '@configs';
import { GrossesForInterval, Load } from '@shared/models';
import { ApiService } from '@shared/services';
import { DateUtils } from '@shared/utils';
import * as mapboxgl from 'mapbox-gl';

@Component({
  selector: 'app-dashboard-page',
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class DashboardPageComponent implements OnInit {
  private map!: mapboxgl.Map;
  public todayGross: number;
  public weeklyGross: number;
  public weeklyDistance: number;
  public rpm: number;
  public loadingLoads: boolean;
  public loads: Load[];
  public chartData: any;
  public chartOptions: any;

  constructor(
    private apiService: ApiService,
    private dateUtils: DateUtils) 
  {
    this.loads = [];
    this.loadingLoads = false;
    this.todayGross = 0;
    this.weeklyGross = 0;
    this.weeklyDistance = 0;
    this.rpm = 0;

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Daily Grosses',
          data: []
        }
      ]
    },

    this.chartOptions = {
      plugins: {
        legend: {
          display: false
        }
      }
    }
  }

  public ngOnInit() {
    this.initMapbox();
    this.fetchLatestLoads();
    this.fetchLastTenDaysGross();
  }

  private initMapbox() {
    this.map = new mapboxgl.Map({
      container: 'map',
      accessToken: AppConfig.mapboxToken,
      style: 'mapbox://styles/mapbox/streets-v11',
      center: [-74.5, 40],
      zoom: 6
    });
  }

  private fetchLatestLoads() {
    this.loadingLoads = true;

    this.apiService.getLoads('', '-dispatchedDate')
      .subscribe(result => {
        if (result.success && result.items) {
          this.loads = result.items;
        }

        this.loadingLoads = false;
    });
  }

  private fetchLastTenDaysGross() {
    const oneWeekAgo = this.dateUtils.daysAgo(7);

    this.apiService.getGrossesForInterval(oneWeekAgo)
      .subscribe(result => {
        if (result.success && result.value) {
          const grosses = result.value;
          
          this.weeklyGross = grosses.totalGross;
          this.weeklyDistance = grosses.totalDistance;
          this.rpm = this.weeklyGross / this.weeklyDistance;
          this.drawChart(grosses);
          this.calcTodayGross(grosses);
        }
      });
  }

  private drawChart(grosses: GrossesForInterval) {
    const labels = new Array<string>();
    const data = new Array<number>();
    
    grosses.days.forEach(i => {
      labels.push(this.dateUtils.toLocaleDate(i.date));
      data.push(i.gross);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: 'Daily Gross',
          data: data,
          fill: true,
          borderColor: '#FFA726',
          tension: 0.4,
          backgroundColor: '#F8E6CA'
        }
      ]
    }
  }

  private calcTodayGross(grosses: GrossesForInterval) {
    const today = new Date();
    let totalGross = 0;

    grosses.days
      .filter(i => this.dateUtils.getDay(i.date) === today.getDay())
      .forEach(i => totalGross += i.gross);

    this.todayGross = totalGross;
  }
}
