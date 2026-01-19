import { DatePipe } from "@angular/common";
import { Component, type OnInit, computed, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  type LoadBoardConfigurationDto,
  type LoadBoardListingDto,
  type PostedTruckDto,
  getLoadBoardProviders,
  getPostedTrucks,
  searchLoadBoard,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DashboardCard, PageHeader, StatCard } from "@/shared/components";

@Component({
  selector: "app-load-board-dashboard",
  templateUrl: "./load-board-dashboard.html",
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    TagModule,
    DatePipe,
    ProgressSpinnerModule,
    PageHeader,
    StatCard,
    DashboardCard,
  ],
})
export class LoadBoardDashboardComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly providers = signal<LoadBoardConfigurationDto[]>([]);
  protected readonly recentListings = signal<LoadBoardListingDto[]>([]);
  protected readonly postedTrucks = signal<PostedTruckDto[]>([]);

  protected readonly activeProviders = computed(() => this.providers().filter((p) => p.isActive));

  protected readonly availableLoads = computed(() => this.recentListings().length);

  protected readonly activePostedTrucks = computed(
    () => this.postedTrucks().filter((t) => t.status === "available").length,
  );

  ngOnInit(): void {
    this.loadDashboardData();
  }

  protected async loadDashboardData(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const [providersData, postedTrucksData] = await Promise.all([
        this.api.invoke(getLoadBoardProviders),
        this.api.invoke(getPostedTrucks, {}),
      ]);

      this.providers.set(providersData ?? []);
      this.postedTrucks.set(postedTrucksData ?? []);

      // If we have active providers, fetch some sample listings
      if (this.activeProviders().length > 0) {
        try {
          const result = await this.api.invoke(searchLoadBoard, {
            body: {
              maxResults: 5,
            },
          });
          this.recentListings.set(result?.listings ?? []);
        } catch {
          // Search may fail without origin, that's okay
        }
      }
    } catch (err) {
      this.error.set("Failed to load dashboard data");
      console.error("Error loading dashboard:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected getProviderStatusSeverity(isActive: boolean): "success" | "danger" {
    return isActive ? "success" : "danger";
  }

  protected configureProviders(): void {
    this.router.navigate(["/loadboard/providers"]);
  }

  protected searchLoads(): void {
    this.router.navigate(["/loadboard/search"]);
  }

  protected managePostedTrucks(): void {
    this.router.navigate(["/loadboard/posted-trucks"]);
  }
}
