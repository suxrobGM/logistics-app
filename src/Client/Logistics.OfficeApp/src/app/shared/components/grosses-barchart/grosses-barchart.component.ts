import {Component, EventEmitter, Input, OnInit, Output, SimpleChanges} from '@angular/core';
import {CommonModule} from '@angular/common';
import {CardModule} from 'primeng/card';
import {ChartModule} from 'primeng/chart';
import {SkeletonModule} from 'primeng/skeleton';
import {MonthlyGrosses} from '@core/models';
import {DateUtils, DistanceConverter} from '@shared/utils';
import {ApiService} from '@core/services';
import {RangeCalendarComponent} from '../range-calendar/range-calendar.component';


@Component({
  selector: 'app-grosses-barchart',
  standalone: true,
  templateUrl: './grosses-barchart.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    CardModule,
    SkeletonModule,
    ChartModule,
    RangeCalendarComponent,
  ],
})
export class GrossesBarchartComponent implements OnInit {
  public isLoading: boolean;
  public monthlyGrosses?: MonthlyGrosses;
  public chartData: any;
  public chartOptions: any;
  public startDate: Date;
  public endDate: Date;

  @Input() truckId?: string;
  @Input() chartColor: string;
  @Output() chartDrawn = new EventEmitter<BarChartDrawnEvent>;

  constructor(private apiService: ApiService) {
    this.isLoading = false;
    this.chartColor = '#EC407A';
    this.startDate = DateUtils.thisYear();
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
          label: 'Monthly Gross',
          data: [],
        },
      ],
    };
  }

  ngOnInit(): void {
    this.fetchMonthlyGrosses();
  }

  fetchMonthlyGrosses() {
    this.isLoading = true;

    this.apiService.getMonthlyGrosses(this.startDate, this.endDate, this.truckId).subscribe((result) => {
      if (result.success && result.value) {
        this.monthlyGrosses = result.value;
        const rpm = this.monthlyGrosses.totalIncome / DistanceConverter.metersTo(this.monthlyGrosses.totalDistance, 'mi');

        this.drawChart(this.monthlyGrosses);
        this.chartDrawn.emit({monthlyGrosses: this.monthlyGrosses, rpm: rpm});
      }

      this.isLoading = false;
    });
  }

  private drawChart(grosses: MonthlyGrosses) {
    const labels: Array<string> = [];
    const data: Array<number> = [];

    grosses.data.forEach((i) => {
      labels.push(DateUtils.monthName(i.month));
      data.push(i.income);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: 'Monthly Gross',
          data: data,
          fill: true,
          tension: 0.4,
          backgroundColor: this.chartColor,
        },
      ],
    };
  }
}

export interface BarChartDrawnEvent {
  monthlyGrosses: MonthlyGrosses;
  rpm: number;
}
