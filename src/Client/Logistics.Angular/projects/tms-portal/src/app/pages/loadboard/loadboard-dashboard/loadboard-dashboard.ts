import { DatePipe } from "@angular/common";
import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { Api, searchLoadBoard, type LoadBoardListingDto } from "@logistics/shared/api";
import { Grid, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DashboardCard, EmptyState, ErrorState, PageHeader, StatCard } from "@/shared/components";
import { LoadBoardQuickActions } from "../_components";
import { LoadBoardStore } from "../store";

@Component({
  selector: "app-loadboard-dashboard",
  templateUrl: "./loadboard-dashboard.html",
  imports: [
    ButtonModule,
    DashboardCard,
    DatePipe,
    EmptyState,
    ErrorState,
    Grid,
    LoadBoardQuickActions,
    PageHeader,
    Stack,
    StatCard,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class LoadBoardDashboardComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  protected readonly store = inject(LoadBoardStore);

  protected readonly recentListings = signal<LoadBoardListingDto[]>([]);
  protected readonly availableLoads = computed(() => this.recentListings().length);

  ngOnInit(): void {
    void this.loadDashboardData();
  }

  protected async loadDashboardData(): Promise<void> {
    await this.store.loadAll(true);

    if (this.store.hasActiveProviders()) {
      try {
        const result = await this.api.invoke(searchLoadBoard, { body: { maxResults: 5 } });
        this.recentListings.set(result?.listings ?? []);
      } catch {
        // Search may fail without origin, that's okay
      }
    }
  }

  protected searchLoads(): void {
    this.router.navigateByUrl("/loadboard/search");
  }

  protected managePostedTrucks(): void {
    this.router.navigateByUrl("/loadboard/posted-trucks");
  }

  protected configureProviders(): void {
    this.router.navigateByUrl("/loadboard/providers");
  }
}
