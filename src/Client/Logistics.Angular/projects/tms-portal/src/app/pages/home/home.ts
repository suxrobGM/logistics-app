import { Component, computed, effect, inject, signal, type OnInit } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Api,
  getCompanyStats,
  getLoads,
  type Address,
  type CompanyStatsDto,
  type LoadDto,
} from "@logistics/shared/api";
import { Icon, Stack, StatCard, StatusBadge, Typography } from "@logistics/shared/components";
import {
  AddressPipe,
  CurrencyFormatPipe,
  DateFormatPipe,
  DistanceUnitPipe,
} from "@logistics/shared/pipes";
import { Gridster, GridsterItem, type GridsterConfig } from "angular-gridster2";
import { SharedModule, type MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { MenuModule } from "primeng/menu";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { AuthService } from "@/core/auth";
import { DashboardSettingsService, type DashboardPanelConfig } from "@/core/services";
import { TrucksMap } from "@/shared/components";
import { Converters } from "@/shared/utils";
import {
  AttentionPanelComponent,
  DailyGrossChartComponent,
  FinancialHealthWidgetComponent,
  LoadProgressBarComponent,
  RecentActivityComponent,
  TopPerformersWidgetComponent,
  type DailyGrossChartData,
} from "./components";
import { HomeSkeleton } from "./home-skeleton/home-skeleton";

@Component({
  selector: "app-home",
  templateUrl: "./home.html",
  styleUrl: "./home.css",
  imports: [
    CardModule,
    SharedModule,
    TableModule,
    RouterLink,
    TooltipModule,
    ButtonModule,
    SkeletonModule,
    CurrencyFormatPipe,
    DateFormatPipe,
    DistanceUnitPipe,
    TrucksMap,
    LoadProgressBarComponent,
    RecentActivityComponent,
    DailyGrossChartComponent,
    AttentionPanelComponent,
    FinancialHealthWidgetComponent,
    TopPerformersWidgetComponent,
    DividerModule,
    MenuModule,
    StatCard,
    StatusBadge,
    Icon,
    Stack,
    Typography,
    Gridster,
    GridsterItem,
    HomeSkeleton,
  ],
  providers: [AddressPipe],
})
export class Home implements OnInit {
  private readonly api = inject(Api);
  private readonly addressPipe = inject(AddressPipe);
  private readonly authService = inject(AuthService);
  protected readonly dashboardSettings = inject(DashboardSettingsService);

  protected readonly todayGross = signal(0);
  protected readonly weeklyGross = signal(0);
  protected readonly weeklyDistance = signal(0);
  protected readonly weeklyRpm = signal(0);
  protected readonly isLoadingLoadsData = signal(false);
  protected readonly isLoadingCompanyStats = signal(false);
  protected readonly initialLoadComplete = signal(false);
  protected readonly companyStats = signal<CompanyStatsDto | null>(null);
  protected readonly loads = signal<LoadDto[]>([]);
  protected readonly recentLoads = signal<LoadDto[]>([]);
  protected readonly lastUpdated = signal<Date>(new Date());
  protected readonly currentDate = new Date();

  /** Visible panels filtered by role + feature. Template consumes this. */
  protected readonly dashboardPanels = this.dashboardSettings.visiblePanels;

  /** Owner-derived KPI computeds (only relevant when owner panels visible). */
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

  /** Gridster configuration */
  protected readonly gridsterOptions: GridsterConfig = {
    gridType: "fit",
    fixedRowHeight: 120,
    minCols: 12,
    maxCols: 12,
    minRows: 1,
    maxRows: 100,
    margin: 8,
    outerMargin: false,
    mobileBreakpoint: 768,
    pushItems: true,
    swap: false,
    draggable: {
      enabled: true,
      ignoreContentClass: "no-drag",
    },
    resizable: {
      enabled: true,
    },
    displayGrid: "onDrag&Resize",
    itemChangeCallback: this.onPanelChange.bind(this),
  };

  private onPanelChange(item: unknown): void {
    const panel = item as DashboardPanelConfig;
    this.dashboardSettings.updatePanelLayout(panel.id, panel.x, panel.y, panel.cols, panel.rows);
  }

  protected readonly greeting = computed(() => {
    const hour = new Date().getHours();
    const name = this.authService.getUserData()?.firstName || "there";
    if (hour < 12) return `Good morning, ${name}`;
    if (hour < 17) return `Good afternoon, ${name}`;
    return `Good evening, ${name}`;
  });

  protected readonly quickActions: MenuItem[] = [
    { label: "Create Load", icon: "pi pi-plus", routerLink: "/loads/add" },
    { label: "View All Loads", icon: "pi pi-list", routerLink: "/loads" },
    { separator: true },
    { label: "View Trucks", icon: "pi pi-truck", routerLink: "/trucks" },
    { label: "Messages", icon: "pi pi-envelope", routerLink: "/messages" },
  ];

  constructor() {
    // Re-fetch company stats whenever the visible panels start including an owner panel
    // (e.g. once auth resolves and the user is detected as Owner).
    effect(() => {
      if (this.dashboardSettings.hasOwnerPanels()) {
        this.fetchCompanyStats();
      }
    });
  }

  ngOnInit(): void {
    this.refreshData();
  }

  protected formatAddress(addressObj: Address): string {
    return this.addressPipe.transform(addressObj) || "No address provided";
  }

  protected refreshData(): void {
    this.lastUpdated.set(new Date());
    this.fetchActiveLoads();
    this.fetchRecentLoads();
    if (this.dashboardSettings.hasOwnerPanels()) {
      this.fetchCompanyStats();
    }
  }

  protected resetLayout(): void {
    this.dashboardSettings.resetToDefaults();
  }

  protected onChartDataLoaded(data: DailyGrossChartData): void {
    this.weeklyGross.set(data.totalGross);
    this.weeklyDistance.set(data.totalDistance);
    this.weeklyRpm.set(data.rpm);
    this.todayGross.set(data.todayGross);
  }

  private calcRpm(gross?: number, distance?: number): number {
    if (gross == null || distance == null || distance === 0) return 0;
    return gross / Converters.metersTo(distance, "mi");
  }

  private async fetchActiveLoads(): Promise<void> {
    this.isLoadingLoadsData.set(true);
    const result = await this.api.invoke(getLoads, {
      OrderBy: "-DispatchedAt",
      OnlyActiveLoads: true,
    });
    if (result) {
      this.loads.set(result.items ?? []);
    }
    this.isLoadingLoadsData.set(false);
    this.initialLoadComplete.set(true);
  }

  private async fetchRecentLoads(): Promise<void> {
    const result = await this.api.invoke(getLoads, {
      OrderBy: "-CreatedAt",
      PageSize: 10,
    });
    if (result) {
      this.recentLoads.set(result.items ?? []);
    }
  }

  private async fetchCompanyStats(): Promise<void> {
    if (this.companyStats() !== null) return; // already fetched this session
    this.isLoadingCompanyStats.set(true);
    const result = await this.api.invoke(getCompanyStats, {});
    if (result) {
      this.companyStats.set(result);
    }
    this.isLoadingCompanyStats.set(false);
  }
}
