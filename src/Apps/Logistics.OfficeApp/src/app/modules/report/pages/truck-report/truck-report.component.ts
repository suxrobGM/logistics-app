import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DailyGrosses, Truck, TruckGrosses } from '@shared/models';
import { DistanceUnitPipe } from '@shared/pipes';
import { ApiService } from '@shared/services';
import { DateUtils } from '@shared/utils';

@Component({
  selector: 'app-truck-report',
  templateUrl: './truck-report.component.html',
  styleUrls: ['./truck-report.component.scss']
})
export class TruckReportComponent implements OnInit {
  public id!: string;
  public isLoadingData: boolean;
  public isLoadingChartData: boolean;
  public truck?: Truck;
  public truckGrosses?: TruckGrosses;
  public rpmCurrent: number;
  public rpmAllTime: number;
  public chartData: any;
  public chartOptions: any;

  constructor(
    private apiService: ApiService,
    private route: ActivatedRoute,
    private dateUtils: DateUtils,
    private distanceUnit: DistanceUnitPipe) 
  {
    this.isLoadingData = false;
    this.isLoadingChartData = false;
    this.rpmCurrent = 0;
    this.rpmAllTime = 0;

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

  public ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.id = params['id'];
    });

    this.fetchTruck();
    this.fetchTruckGrosses();
  }

  private fetchTruck() {
    this.isLoadingData = true;

    this.apiService.getTruck(this.id).subscribe(result => {
      if (result.success && result.value) {
        this.truck = result.value;
      }

      this.isLoadingData = false;
    });
  }

  private fetchTruckGrosses() {
    this.isLoadingChartData = true;
    const oneMonthAgo = this.dateUtils.daysAgo(30);

    this.apiService.getTruckGrosses(this.id, oneMonthAgo).subscribe(result => {
      if (result.success && result.value) {
        const truckGrosses = result.value;
        this.truckGrosses = result.value;
        this.rpmAllTime = truckGrosses.incomeAllTime / this.toMi(truckGrosses.distanceAllTime);

        if (truckGrosses.grosses) {
          this.rpmCurrent = truckGrosses.grosses.totalIncome / this.toMi(truckGrosses.grosses.totalDistance);
        }
        
        this.drawChart(truckGrosses.grosses!);
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

  private toMi(value?: number): number {
    return this.distanceUnit.transform(value, 'mi');
  }
}
