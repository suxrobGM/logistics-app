import { Component, inject, signal } from "@angular/core";
import { Api, getCompanyStats, type CompanyStatsDto } from "@logistics/shared/api";
import { Grid, PageHeader } from "@logistics/shared/components";
import { GrossBarchart, GrossSummaryWidgetComponent } from "@/shared/components";

@Component({
  selector: "app-revenue-report",
  templateUrl: "./revenue-report.html",
  imports: [PageHeader, Grid, GrossSummaryWidgetComponent, GrossBarchart],
})
export class RevenueReportComponent {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly companyStats = signal<CompanyStatsDto | null>(null);

  constructor() {
    this.fetchStats();
  }

  private async fetchStats(): Promise<void> {
    this.isLoading.set(true);
    const result = await this.api.invoke(getCompanyStats, {});
    if (result) {
      this.companyStats.set(result);
    }
    this.isLoading.set(false);
  }
}
