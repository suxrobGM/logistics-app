import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getTrucksStats$Json } from "@/core/api";
import type { TruckStatsDto } from "@/core/api/models";
import { RangeCalendar } from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import { DateUtils } from "@/shared/utils";

@Component({
  selector: "app-truck-stats-table",
  templateUrl: "./truck-stats-table.html",
  imports: [
    CommonModule,
    CurrencyPipe,
    TableModule,
    RouterLink,
    DatePipe,
    DistanceUnitPipe,
    CardModule,
    ButtonModule,
    RangeCalendar,
    TooltipModule,
  ],
})
export class TruckStatsTableComponent {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly truckStats = signal<TruckStatsDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly startDate = signal<Date>(DateUtils.daysAgo(30));
  protected readonly endDate = signal<Date>(DateUtils.today());

  protected reloadTable(): void {
    this.fetchTrucksStats({ first: 0, rows: 10 });
  }

  protected async fetchTrucksStats(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getTrucksStats$Json, {
      StartDate: this.startDate().toISOString(),
      EndDate: this.endDate().toISOString(),
      OrderBy: sortField,
      Page: page,
      PageSize: rows,
    });
    if (result) {
      this.truckStats.set(result.items ?? []);
      this.totalRecords.set(result.pagination?.total ?? 0);
    }

    this.isLoading.set(false);
  }
}
