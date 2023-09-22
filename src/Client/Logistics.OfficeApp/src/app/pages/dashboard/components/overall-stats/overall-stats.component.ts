import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {OverallStats} from '@core/models';
import {ApiService} from '@core/services';
import {DistanceUtils} from '@shared/utils';
import {DistanceUnitPipe} from '@shared/pipes';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {ChartModule} from 'primeng/chart';
import {SkeletonModule} from 'primeng/skeleton';


@Component({
  selector: 'app-overall-stats',
  standalone: true,
  templateUrl: './overall-stats.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    DistanceUnitPipe,
    CurrencyPipe,
    CardModule,
    CommonModule,
    SkeletonModule,
    SharedModule,
    ChartModule,
  ],
})
export class OverallStatsComponent {
  public isLoadingOverallStats: boolean;
  public rpm: number;
  public overallStats?: OverallStats;

  constructor(private apiService: ApiService) {
    this.isLoadingOverallStats = false;
    this.rpm = 0;
  }

  ngOnInit(): void {
    this.fetchOverallStats();
  }

  private fetchOverallStats() {
    this.isLoadingOverallStats = true;

    this.apiService.getOverallStats().subscribe((result) => {
      if (result.success && result.value) {
        const stats = result.value;
        this.overallStats = result.value;
        this.rpm = stats.totalIncome / DistanceUtils.metersTo(stats.totalDistance, 'mi');
      }

      this.isLoadingOverallStats = false;
    });
  }
}
