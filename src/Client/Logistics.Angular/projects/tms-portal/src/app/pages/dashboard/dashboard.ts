import { Component, computed, inject, signal } from "@angular/core";
import { Api, getCompanyStats } from "@logistics/shared/api";
import type { CompanyStatsDto } from "@logistics/shared/api";
import { StatCard } from "@logistics/shared/components";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { MenuModule } from "primeng/menu";
import { TooltipModule } from "primeng/tooltip";
import { AuthService } from "@/core/auth";
import { GrossBarchart, TrucksMap } from "@/shared/components";
import { Converters } from "@/shared/utils";
import {
  AttentionPanelComponent,
  FinancialHealthWidgetComponent,
  GrossSummaryWidgetComponent,
  TeamOverviewWidgetComponent,
  TopPerformersWidgetComponent,
} from "./components";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.html",
  imports: [
    GrossBarchart,
    TrucksMap,
    AttentionPanelComponent,
    FinancialHealthWidgetComponent,
    TopPerformersWidgetComponent,
    GrossSummaryWidgetComponent,
    TeamOverviewWidgetComponent,
    StatCard,
    CurrencyFormatPipe,
    DateFormatPipe,
    ButtonModule,
    MenuModule,
    TooltipModule,
  ],
})
export class DashboardComponent {
  private readonly api = inject(Api);
  private readonly authService = inject(AuthService);

  protected readonly isLoading = signal(false);
  protected readonly companyStats = signal<CompanyStatsDto | null>(null);
  protected readonly currentDate = new Date();
  protected readonly lastUpdated = signal<Date>(new Date());

  protected readonly greeting = computed(() => {
    const hour = new Date().getHours();
    const name = this.authService.getUserData()?.firstName || "there";
    if (hour < 12) return `Good morning, ${name}`;
    if (hour < 17) return `Good afternoon, ${name}`;
    return `Good evening, ${name}`;
  });

  protected readonly quickActions: MenuItem[] = [
    { label: "Create Load", icon: "pi pi-plus", routerLink: "/loads/add" },
    { label: "Create Invoice", icon: "pi pi-file", routerLink: "/invoices/add" },
    { separator: true },
    { label: "View Reports", icon: "pi pi-chart-bar", routerLink: "/reports" },
  ];

  // KPI Calculations
  protected readonly thisWeekGrossTrend = computed(() => {
    const stats = this.companyStats();
    if (!stats?.thisWeekGross || !stats?.lastWeekGross || stats.lastWeekGross === 0) return null;
    const change = ((stats.thisWeekGross - stats.lastWeekGross) / stats.lastWeekGross) * 100;
    return {
      value: `${change >= 0 ? "+" : ""}${change.toFixed(1)}%`,
      direction: change >= 0 ? ("up" as const) : ("down" as const),
    };
  });

  protected readonly rpmTrend = computed(() => {
    const stats = this.companyStats();
    if (!stats) return null;

    const thisWeekRpm = this.calcRpm(stats.thisWeekGross, stats.thisWeekDistance);
    const lastWeekRpm = this.calcRpm(stats.lastWeekGross, stats.lastWeekDistance);

    if (lastWeekRpm === 0) return null;
    const change = ((thisWeekRpm - lastWeekRpm) / lastWeekRpm) * 100;
    return {
      value: `${change >= 0 ? "+" : ""}${change.toFixed(1)}%`,
      direction: change >= 0 ? ("up" as const) : ("down" as const),
    };
  });

  protected readonly thisWeekRpm = computed(() => {
    const stats = this.companyStats();
    if (!stats) return 0;
    return this.calcRpm(stats.thisWeekGross, stats.thisWeekDistance);
  });

  constructor() {
    this.fetchCompanyStats();
  }

  protected refreshData(): void {
    this.lastUpdated.set(new Date());
    this.fetchCompanyStats();
  }

  private calcRpm(gross?: number, distance?: number): number {
    if (gross == null || distance == null || distance === 0) return 0;
    return gross / Converters.metersTo(distance, "mi");
  }

  private async fetchCompanyStats(): Promise<void> {
    this.isLoading.set(true);
    const result = await this.api.invoke(getCompanyStats, {});
    if (result) {
      this.companyStats.set(result);
    }
    this.isLoading.set(false);
  }
}
