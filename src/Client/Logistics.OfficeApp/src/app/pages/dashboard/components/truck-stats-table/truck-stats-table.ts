import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ApiService } from "@/core/api";
import { PagedIntervalQuery, TruckStatsDto } from "@/core/api/models";
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
  private readonly apiService = inject(ApiService);

  protected readonly isLoading = signal(false);
  protected readonly truckStats = signal<TruckStatsDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly startDate = signal<Date>(DateUtils.daysAgo(30));
  protected readonly endDate = signal<Date>(DateUtils.today());

  protected reloadTable(): void {
    this.fetchTrucksStats({ first: 0, rows: 10 });
  }

  protected fetchTrucksStats(event: TableLazyLoadEvent): void {
    this.isLoading.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.formatSortField(event.sortField as string, event.sortOrder);

    const query: PagedIntervalQuery = {
      startDate: this.startDate(),
      endDate: this.endDate(),
      orderBy: sortField,
      page: page,
      pageSize: rows,
    };

    this.apiService.statsApi.getTrucksStats(query).subscribe((result) => {
      if (result.success && result.data) {
        this.truckStats.set(result.data);
        this.totalRecords.set(result.totalItems);
      }

      this.isLoading.set(false);
    });
  }
}
