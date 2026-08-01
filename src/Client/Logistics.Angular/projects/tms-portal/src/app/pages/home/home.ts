import { NgTemplateOutlet } from "@angular/common";
import { Component, computed, effect, inject, signal, untracked, type OnInit } from "@angular/core";
import {
  Api,
  getCompanyStats,
  getDailyGrosses,
  getLoads,
  type CompanyStatsDto,
  type DailyGrossesDto,
  type LoadDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { LayoutService } from "@logistics/shared/services";
import {
  Card,
  Divider,
  Icon,
  Stack,
  Typography,
  UiButton,
  UiMenu,
  UiTooltip,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { Gridster, GridsterItem, type GridsterConfig } from "angular-gridster2";
import { AuthService } from "@/core/auth";
import {
  DashboardSettingsService,
  TenantService,
  type DashboardPanelConfig,
} from "@/core/services";
import { PageHeader, StatCard, TrucksMap } from "@/shared/components";
import {
  ActiveLoadsPanel,
  AttentionPanelComponent,
  DailyGrossChartComponent,
  FinancialHealthWidgetComponent,
  HosRemaining,
  OnboardingChecklist,
  RecentActivityComponent,
  TopPerformersWidgetComponent,
} from "./components";
import { HomeSkeleton } from "./home-skeleton/home-skeleton";
import { summarizeDailyGrosses, weeklyGrossStartDate } from "./utils/weekly-gross";

@Component({
  selector: "app-home",
  templateUrl: "./home.html",
  styleUrl: "./home.css",
  imports: [
    ActiveLoadsPanel,
    AttentionPanelComponent,
    Card,
    CurrencyFormatPipe,
    DailyGrossChartComponent,
    DateFormatPipe,
    DistanceUnitPipe,
    Divider,
    FinancialHealthWidgetComponent,
    Gridster,
    GridsterItem,
    HomeSkeleton,
    HosRemaining,
    Icon,
    NgTemplateOutlet,
    OnboardingChecklist,
    PageHeader,
    RecentActivityComponent,
    Stack,
    StatCard,
    TopPerformersWidgetComponent,
    TrucksMap,
    Typography,
    UiButton,
    UiMenu,
    UiTooltip,
  ],
})
export class Home implements OnInit {
  private readonly api = inject(Api);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  protected readonly dashboardSettings = inject(DashboardSettingsService);
  private readonly layout = inject(LayoutService);

  private readonly dailyGrosses = signal<DailyGrossesDto | null>(null);
  protected readonly weeklyKpis = computed(() => summarizeDailyGrosses(this.dailyGrosses()));
  protected readonly isLoadingLoadsData = signal(false);
  protected readonly isLoadingCompanyStats = signal(false);
  protected readonly initialLoadComplete = signal(false);
  protected readonly companyStats = signal<CompanyStatsDto | null>(null);
  protected readonly loads = signal<LoadDto[]>([]);
  protected readonly recentLoads = signal<LoadDto[]>([]);
  protected readonly lastUpdated = signal<Date>(new Date());
  protected readonly currentDate = new Date();

  protected readonly dashboardPanels = this.dashboardSettings.visiblePanels;

  /** Too narrow for a 12-column grid beside the sidebar - render the panels as a plain stack. */
  protected readonly stackPanels = this.layout.isCompact;

  protected readonly activeLoadsTitle = computed(() =>
    this.tenantService.isSoloMode() ? "Your Loads" : "Active Loads",
  );

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
    // Off - it measures the grid's width, not the viewport. `stackPanels` is the sole authority.
    mobileBreakpoint: 0,
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

  protected readonly quickActions: UiMenuItem[] = [
    { label: "Create Load", icon: "plus", routerLink: "/loads/add" },
    { label: "View All Loads", icon: "list", routerLink: "/loads" },
    { separator: true },
    { label: "View Trucks", icon: "truck", routerLink: "/trucks" },
    { label: "Messages", icon: "mail", routerLink: "/messages" },
  ];

  protected readonly panelMenuItems = computed<UiMenuItem[]>(() => {
    const additions = this.dashboardSettings.availablePanels().map<UiMenuItem>((panel) => ({
      label: `Add ${panel.label}`,
      icon: panel.icon,
      command: () => this.dashboardSettings.showPanel(panel.id),
    }));
    const removals = this.dashboardSettings.visiblePanels().map<UiMenuItem>((panel) => ({
      label: `Remove ${panel.label}`,
      icon: "minus",
      variant: "destructive",
      command: () => this.dashboardSettings.hidePanel(panel.id),
    }));
    const separator: UiMenuItem[] =
      additions.length && removals.length ? [{ separator: true }] : [];
    return [...additions, ...separator, ...removals];
  });

  constructor() {
    // Covers both auth resolving the user as Owner and the user adding a stats-backed panel by hand.
    // `untracked` keeps the fetch's own guard signals out of this effect's dependency set.
    effect(() => {
      if (this.dashboardSettings.needsCompanyStats()) {
        untracked(() => this.fetchCompanyStats());
      }
    });
  }

  ngOnInit(): void {
    this.refreshData();
  }

  protected refreshData(): void {
    this.lastUpdated.set(new Date());
    this.fetchActiveLoads();
    this.fetchRecentLoads();
    this.fetchWeeklyKpis();
    if (this.dashboardSettings.needsCompanyStats()) {
      this.fetchCompanyStats(true);
    }
  }

  protected resetLayout(): void {
    this.dashboardSettings.resetToDefaults();
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

  /**
   * Lives here, not on the chart panel, because the KPI cards are default-visible and the chart is
   * not. Both issue the same request, which the HTTP cache then serves once.
   */
  private async fetchWeeklyKpis(): Promise<void> {
    const result = await this.api.invoke(getDailyGrosses, {
      StartDate: weeklyGrossStartDate(),
    });
    if (result) {
      this.dailyGrosses.set(result);
    }
  }

  /**
   * `ngOnInit` and the owner-panel effect both call this on a cold load. The in-flight guard is only
   * sound because `isLoadingCompanyStats` is set before the first await.
   */
  private async fetchCompanyStats(force = false): Promise<void> {
    if (this.isLoadingCompanyStats()) return;
    if (!force && this.companyStats() !== null) return;

    this.isLoadingCompanyStats.set(true);
    const result = await this.api.invoke(getCompanyStats, {});
    if (result) {
      this.companyStats.set(result);
    }
    this.isLoadingCompanyStats.set(false);
  }
}
