import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {CardModule} from 'primeng/card';
import {SkeletonModule} from 'primeng/skeleton';
import {ChartModule} from 'primeng/chart';
import {DailyGrosses} from '@core/models';
import {DateUtils, DistanceUtils} from '@shared/utils';
import {ApiService} from '@core/services';
import {RangeCalendarComponent} from '@shared/components';


@Component({
  selector: 'app-truck-grosses-linechart',
  standalone: true,
  templateUrl: './truck-grosses-linechart.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    CardModule,
    SkeletonModule,
    ChartModule,
    RangeCalendarComponent,
  ],
})
export class TruckGrossesLinechartComponent implements OnInit {
  public isLoading: boolean;
  public dailyGrosses?: DailyGrosses;
  public chartData: any;
  public chartOptions: any;
  public startDate: Date;
  public endDate: Date;

  @Input({required: true}) truckId!: string;
  @Output() chartDrawn = new EventEmitter<LineChartDrawnEvent>;

  constructor(private apiService: ApiService) {
    this.isLoading = false;
    this.startDate = DateUtils.daysAgo(30);
    this.endDate = DateUtils.today();

    this.chartOptions = {
      plugins: {
        legend: {
          display: false,
        },
      },
    };

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Daily Gross',
          data: [],
        },
      ],
    };
  }

  ngOnInit(): void {
    this.fetchDailyGrosses();
  }

  fetchDailyGrosses() {
    this.isLoading = true;

    this.apiService.getDailyGrosses(this.startDate, this.endDate, this.truckId).subscribe((result) => {
      if (result.success && result.value) {
        this.dailyGrosses = result.value;
        const rpm = this.dailyGrosses.totalIncome / DistanceUtils.metersTo(this.dailyGrosses.totalDistance, 'mi');

        this.drawChart(this.dailyGrosses);
        this.chartDrawn.emit({dailyGrosses: this.dailyGrosses, rpm: rpm});
      }

      this.isLoading = false;
    });
  }

  private drawChart(grosses: DailyGrosses) {
    const labels: Array<string> = [];
    const data: Array<number> = [];

    grosses.dates.forEach((i) => {
      labels.push(DateUtils.toLocaleDate(i.date));
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
          backgroundColor: '#88a5d3',
        },
      ],
    };
  }
}

export interface LineChartDrawnEvent {
  dailyGrosses: DailyGrosses;
  rpm: number;
}
