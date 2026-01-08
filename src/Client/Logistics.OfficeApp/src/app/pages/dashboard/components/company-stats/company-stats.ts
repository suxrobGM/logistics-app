import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { SharedModule } from "primeng/api";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { Api, getCompanyStats$Json } from "@/core/api";
import type { CompanyStatsDto } from "@/core/api/models";
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
  private readonly api = inject(Api);

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

  private async fetchCompanyStats(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getCompanyStats$Json, {});
    if (result) {
      this.companyStats.set(result);
      this.rpm.set((result.totalGross ?? 0) / Converters.metersTo(result.totalDistance ?? 0, "mi"));
    }

    this.isLoading.set(false);
  }
}
