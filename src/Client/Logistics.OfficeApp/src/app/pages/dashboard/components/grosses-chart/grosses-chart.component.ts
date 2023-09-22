import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {ChartModule} from 'primeng/chart';
import {SkeletonModule} from 'primeng/skeleton';
import {MonthlyGrosses} from '@core/models';
import {ApiService} from '@core/services';
import {DateUtils} from '@shared/utils';


@Component({
  selector: 'app-grosses-chart',
  standalone: true,
  templateUrl: './grosses-chart.component.html',
  styleUrls: [],
  imports: [
    CardModule,
    CommonModule,
    SkeletonModule,
    SharedModule,
    ChartModule,
  ],
})
export class GrossesChartComponent implements OnInit {
  public isLoading: boolean;
  public chartData: any;
  public chartOptions: any;

  constructor(private apiService: ApiService) {
    this.isLoading = false;

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Monthly Gross',
          data: [],
        },
      ],
    };

    this.chartOptions = {
      plugins: {
        legend: {
          display: false,
        },
      },
    };
  }
  ngOnInit(): void {
    this.fetchMonthlyGrosses();
  }

  private fetchMonthlyGrosses() {
    this.isLoading = true;
    const thisYear = DateUtils.thisYear();

    this.apiService.getMonthlyGrosses(thisYear).subscribe((result) => {
      if (result.success && result.value) {
        const monthlyGrosses = result.value;
        this.drawChart(monthlyGrosses);
      }

      this.isLoading = false;
    });
  }

  private drawChart(grosses: MonthlyGrosses) {
    const labels: Array<string> = [];
    const data: Array<number> = [];

    grosses.months.forEach((i) => {
      labels.push(DateUtils.getMonthName(i.month));
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
          backgroundColor: '#88a5d3',
        },
      ],
    };
  }
}
