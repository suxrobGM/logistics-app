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
export class OverviewComponent implements OnInit {
  public loadingData: boolean;
  public loadingChart: boolean;
  public rpm: number;
  public overallStats?: OverallStats;
  public chartData: any;
  public chartOptions: any;

  constructor(
    private apiService: ApiService,
    private distanceUnit: DistanceUnitPipe
  )
  {
    this.loadingData = false;
    this.loadingChart = false;
    this.rpm = 0;

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: 'Monthly Gross',
          data: [] 
        }
      ]
    }

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
    this.loadingData = true;

    this.apiService.getOverallStats().subscribe(result => {
      if (result.success && result.value) {
        const stats = result.value;
        this.overallStats = result.value;
        this.rpm = stats.totalIncome / this.toMi(stats.totalDistance);
      }

      this.loadingData = false;
    });
  }

  private fetchMonthlyGrosses() {
    this.loadingChart = true;
    const thisYear = DateUtils.thisYear();

    this.apiService.getMonthlyGrosses(thisYear).subscribe(result => {
      if (result.success && result.value) {
        const monthlyGrosses = result.value;
        this.drawChart(monthlyGrosses);
      }

      this.loadingChart = false;
    });
  }

  private drawChart(grosses: MonthlyGrosses) {
    const labels = new Array<string>();
    const data = new Array<number>();

    grosses.months.forEach(i => {
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
          backgroundColor: '#88a5d3'
        }
      ]
    }
  }

  private toMi(value?: number): number {
    return this.distanceUnit.transform(value, 'mi');
  }
}
