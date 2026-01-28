import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, computed, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, getLoads } from "@logistics/shared/api";
import type { Address, LoadDto } from "@logistics/shared/api";
import { StatCard } from "@logistics/shared/components";
import { AddressPipe } from "@logistics/shared/pipes";
import { SharedModule } from "primeng/api";
import type { MenuItem } from "primeng/api";
import { BadgeModule } from "primeng/badge";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { MenuModule } from "primeng/menu";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { AuthService } from "@/core/auth";
import { DashboardSettingsService, type PanelType } from "@/core/services";
import { TrucksMap } from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import {
  DailyGrossChartComponent,
  type DailyGrossChartData,
  DashboardSettingsDialog,
  LoadProgressBarComponent,
  RecentActivityComponent,
} from "./components";

@Component({
  selector: "app-home",
  templateUrl: "./home.html",
  imports: [
    CardModule,
    SharedModule,
    TableModule,
    RouterLink,
    TooltipModule,
    ButtonModule,
    SkeletonModule,
    CurrencyPipe,
    DatePipe,
    DistanceUnitPipe,
    TrucksMap,
    LoadProgressBarComponent,
    RecentActivityComponent,
    DailyGrossChartComponent,
    DividerModule,
    BadgeModule,
    MenuModule,
    StatCard,
    DashboardSettingsDialog,
  ],
  providers: [AddressPipe],
})
export class HomeComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly addressPipe = inject(AddressPipe);
  private readonly authService = inject(AuthService);
  private readonly dashboardSettings = inject(DashboardSettingsService);

  protected readonly showSettingsDialog = signal(false);
  protected readonly todayGross = signal(0);
  protected readonly weeklyGross = signal(0);
  protected readonly weeklyDistance = signal(0);
  protected readonly weeklyRpm = signal(0);
  protected readonly isLoadingLoadsData = signal(false);
  protected readonly loads = signal<LoadDto[]>([]);
  protected readonly recentLoads = signal<LoadDto[]>([]);
  protected readonly lastUpdated = signal<Date>(new Date());
  protected readonly currentDate = new Date();

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

  ngOnInit(): void {
    this.refreshData();
  }

  protected formatAddress(addressObj: Address): string {
    return this.addressPipe.transform(addressObj) || "No address provided";
  }

  protected getStatusSeverity(
    status: LoadDto["status"],
  ): "info" | "warn" | "success" | "danger" | "secondary" {
    switch (status) {
      case "dispatched":
        return "info";
      case "picked_up":
        return "warn";
      case "delivered":
        return "success";
      case "cancelled":
        return "danger";
      default:
        return "secondary";
    }
  }

  protected getStatusLabel(status: LoadDto["status"]): string {
    switch (status) {
      case "draft":
        return "Draft";
      case "dispatched":
        return "Dispatched";
      case "picked_up":
        return "In Transit";
      case "delivered":
        return "Delivered";
      case "cancelled":
        return "Cancelled";
      default:
        return "Unknown";
    }
  }

  protected refreshData(): void {
    this.lastUpdated.set(new Date());
    this.fetchActiveLoads();
    this.fetchRecentLoads();
  }

  protected isPanelVisible(panelId: PanelType): boolean {
    return this.dashboardSettings.isPanelVisible(panelId);
  }

  protected getPanelClass(panelId: PanelType): string {
    return this.dashboardSettings.getPanelClass(panelId);
  }

  protected onChartDataLoaded(data: DailyGrossChartData): void {
    this.weeklyGross.set(data.totalGross);
    this.weeklyDistance.set(data.totalDistance);
    this.weeklyRpm.set(data.rpm);
    this.todayGross.set(data.todayGross);
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
}
