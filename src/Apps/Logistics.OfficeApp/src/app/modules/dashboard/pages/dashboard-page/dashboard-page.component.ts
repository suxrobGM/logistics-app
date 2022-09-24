import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { AppConfig } from '@configs';
import { DailyGrosses, Load } from '@shared/models';
import { DistanceUnitPipe } from '@shared/pipes';
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
  public isLoadingLoads: boolean;
  public isLoadingMap: boolean;
  public isLoadingChartData: boolean;
  public loads: Load[];
  public chartData: any;
  public chartOptions: any;

  constructor(
    private apiService: ApiService,
    private dateUtils: DateUtils,
    private distanceUnit: DistanceUnitPipe) 
  {
    this.loads = [];
    this.isLoadingLoads = false;
    this.isLoadingMap = false;
    this.isLoadingChartData = false;
    this.todayGross = 0;
    this.weeklyGross = 0;
    this.weeklyDistance = 0;
    this.rpm = 0;

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Daily Gross',
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
    this.isLoadingMap = true;

    this.map = new mapboxgl.Map({
      container: 'map',
      accessToken: AppConfig.mapboxToken,
      style: 'mapbox://styles/mapbox/streets-v11',
      center: [-74.5, 40],
      zoom: 6
    });

    this.map.on('load', () => this.isLoadingMap = false);
  }

  private fetchLatestLoads() {
    this.isLoadingLoads = true;

    this.apiService.getLoads('', '-dispatchedDate').subscribe(result => {
      if (result.success && result.items) {
        this.loads = result.items;
      }

      this.isLoadingLoads = false;
    });
  }

  private fetchLastTenDaysGross() {
    this.isLoadingChartData = true;
    const oneWeekAgo = this.dateUtils.daysAgo(7);

    this.apiService.getDailyGrosses(oneWeekAgo).subscribe(result => {
      if (result.success && result.value) {
        const grosses = result.value;
        
        this.weeklyGross = grosses.totalIncome;
        this.weeklyDistance = grosses.totalDistance;
        this.rpm = this.weeklyGross / this.toMi(this.weeklyDistance);
        this.drawChart(grosses);
        this.calcTodayGross(grosses);
      }

      this.isLoadingChartData = false;
    });
  }

  private drawChart(grosses: DailyGrosses) {
    const labels = new Array<string>();
    const data = new Array<number>();
    
    grosses.dates.forEach(i => {
      labels.push(this.dateUtils.toLocaleDate(i.date));
      data.push(i.income);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: 'Daily Gross',
          data: data,
          fill: true,
          tension: 0.4,
          borderColor: '#405a83',
          backgroundColor: '#88a5d3'
        }
      ]
    }
  }

  private calcTodayGross(grosses: DailyGrosses) {
    const today = new Date();
    let totalGross = 0;

    grosses.dates
      .filter(i => this.dateUtils.getDate(i.date) === today.getDate())
      .forEach(i => totalGross += i.income);

    this.todayGross = totalGross;
  }

  private toMi(value?: number): number {
    return this.distanceUnit.transform(value, 'mi');
  }
}
