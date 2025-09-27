import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { SharedModule } from "primeng/api";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { ApiService } from "@/core/api";
import { CompanyStatsDto } from "@/core/api/models";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-company-stats",
  templateUrl: "./company-stats.html",
  imports: [
    CommonModule,
    CurrencyPipe,
    CardModule,
    CommonModule,
    SkeletonModule,
    SharedModule,
    ChartModule,
  ],
})
export class CompanyStatsComponent {
  private readonly apiService = inject(ApiService);

  protected readonly isLoading = signal(false);
  protected readonly rpm = signal(0);
  protected readonly companyStats = signal<CompanyStatsDto | null>(null);

  constructor() {
    this.fetchCompanyStats();
  }

  protected calcRpm(gross?: number, distance?: number): number {
    if (gross == null || distance == null) {
      return 0;
    }

    return gross / Converters.metersTo(distance, "mi");
  }

  private fetchCompanyStats() {
    this.isLoading.set(true);

    this.apiService.statsApi.getCompanyStats().subscribe((result) => {
      if (result.success && result.data) {
        const stats = result.data;
        this.companyStats.set(result.data);
        this.rpm.set(stats.totalGross / Converters.metersTo(stats.totalDistance, "mi"));
      }

      this.isLoading.set(false);
    });
  }
}
