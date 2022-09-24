import { Component, OnInit } from '@angular/core';
import { MonthlyGrosses, OverallStats } from '@shared/models';
import { DistanceUnitPipe } from '@shared/pipes';
import { ApiService } from '@shared/services';
import { DateUtils } from '@shared/utils';

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class ReportPageComponent implements OnInit {
  public isLoadingData: boolean;
  public isLoadingChartData: boolean;
  public rpm: number;
  public overallStats?: OverallStats;
  public chartData: any;
  public chartOptions: any;

  constructor(
    private apiService: ApiService,
    private dateUtils: DateUtils,
    private distanceUnit: DistanceUnitPipe
  )
  {
    this.isLoadingData = false;
    this.isLoadingChartData = false;
    this.rpm = 0;

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Monthly Gross',
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
    this.fetchOverallStats();
    this.fetchMonthlyGrosses();
  }

  private fetchOverallStats() {
    this.isLoadingData = true;

    this.apiService.getOverallStats().subscribe(result => {
      if (result.success && result.value) {
        const stats = result.value;
        this.overallStats = result.value;
        this.rpm = stats.incomeAllTime / this.toMi(stats.distanceAllTime);
      }

      this.isLoadingData = false;
    });
  }

  private fetchMonthlyGrosses() {
    this.isLoadingChartData = true;
    const thisYear = this.dateUtils.thisYear();

    this.apiService.getMonthlyGrosses(thisYear).subscribe(result => {
      if (result.success && result.value) {
        const monthlyGrosses = result.value;
        this.drawChart(monthlyGrosses);
      }

      this.isLoadingChartData = false;
    });
  }

  private drawChart(grosses: MonthlyGrosses) {
    const labels = new Array<string>();
    const data = new Array<number>();

    grosses.months.forEach(i => {
      labels.push(this.dateUtils.getMonthName(i.month));
      data.push(i.income);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: 'Monthly Gross',
          data: data,
          fill: true,
          borderColor: '#EC407A',
          tension: 0.4,
          backgroundColor: '#EC407A'
        }
      ]
    }
  }

  private toMi(value?: number): number {
    return this.distanceUnit.transform(value, 'mi');
  }
}
