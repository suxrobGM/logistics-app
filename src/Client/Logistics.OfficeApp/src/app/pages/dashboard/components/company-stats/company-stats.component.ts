import {Component} from '@angular/core';
import {CommonModule, CurrencyPipe} from '@angular/common';
import {CompanyStats} from '@core/models';
import {ApiService} from '@core/services';
import {DistanceConverter} from '@shared/utils';
import {DistanceUnitPipe} from '@shared/pipes';
import {SharedModule} from 'primeng/api';
import {CardModule} from 'primeng/card';
import {ChartModule} from 'primeng/chart';
import {SkeletonModule} from 'primeng/skeleton';


@Component({
  selector: 'app-company-stats',
  standalone: true,
  templateUrl: './company-stats.component.html',
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
export class CompanyStatsComponent {
  public isLoading: boolean;
  public rpm: number;
  public companyStats?: CompanyStats;

  constructor(private apiService: ApiService) {
    this.isLoading = false;
    this.rpm = 0;
  }

  ngOnInit(): void {
    this.fetchCompanyStats();
  }

  calcRpm(gross?: number, distance?: number): number {
    if (gross == null || distance == null) {
      return 0;
    }

    return gross / DistanceConverter.metersTo(distance, 'mi');
  }

  private fetchCompanyStats() {
    this.isLoading = true;

    this.apiService.getCompanyStats().subscribe((result) => {
      if (result.success && result.value) {
        const stats = result.value;
        this.companyStats = result.value;
        this.rpm = stats.totalGross / DistanceConverter.metersTo(stats.totalDistance, 'mi');
      }

      this.isLoading = false;
    });
  }
}
