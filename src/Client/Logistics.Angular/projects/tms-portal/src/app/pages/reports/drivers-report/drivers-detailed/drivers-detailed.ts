import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Api, formatSortField, getDriversReport } from "@logistics/shared/api";
import type { DriverReportDto } from "@logistics/shared/api";
import type { PagedResponse } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import { Observable, from } from "rxjs";
import { BaseTableComponent, DateRangePicker, type TableQueryParams } from "@/shared/components";
import { DateUtils } from "@/shared/utils";

@Component({
  selector: "app-drivers-detailed",
  templateUrl: "./drivers-detailed.html",
  imports: [
    FormsModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    TableModule,
    DatePickerModule,
    DateRangePicker,
    CurrencyPipe,
    DecimalPipe,
    TagModule,
  ],
})
export class DriversDetailedComponent extends BaseTableComponent<DriverReportDto> {
  private readonly api = inject(Api);

  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());

  protected override query(params: TableQueryParams): Observable<PagedResponse<DriverReportDto>> {
    const orderBy = formatSortField(params.sortField, params.sortOrder);

    return from(
      this.api.invoke(getDriversReport, {
        Page: params.page + 1,
        PageSize: params.size,
        OrderBy: orderBy,
        Search: params.search,
        StartDate: this.startDate().toISOString(),
        EndDate: this.endDate().toISOString(),
      }),
    );
  }
  protected onDateRangeChange(dates: Date[]): void {
    if (dates.length === 2) {
      this.startDate.set(dates[0]);
      this.endDate.set(dates[1]);
      this.fetch({ page: 0, size: 10 });
    }
  }

  protected getPerformanceLevel(efficiency: number): string {
    if (efficiency >= 2.0) return "Excellent";
    if (efficiency >= 1.5) return "Good";
    if (efficiency >= 1.0) return "Average";
    return "Below Average";
  }

  protected getPerformanceSeverity(efficiency: number): Tag["severity"] {
    if (efficiency >= 2.0) return "success";
    if (efficiency >= 1.5) return "info";
    if (efficiency >= 1.0) return "warn";
    return "danger";
  }

  protected getDriverTypeSeverity(isMainDriver: boolean): Tag["severity"] {
    return isMainDriver ? "success" : "info";
  }
}
