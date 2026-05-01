import { Component, inject, signal } from "@angular/core";
import { Api, getCompanyStats, type CompanyStatsDto } from "@logistics/shared/api";
import { PageHeader } from "@logistics/shared/components";
import { TeamOverviewWidgetComponent } from "@/shared/components";

@Component({
  selector: "app-team-report",
  templateUrl: "./team-report.html",
  imports: [PageHeader, TeamOverviewWidgetComponent],
})
export class TeamReportComponent {
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
