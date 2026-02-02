import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LocalizationService } from "@logistics/shared";
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
import { DateUtils, getPerformanceLevel, getPerformanceSeverity } from "@/shared/utils";

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
  private readonly localization = inject(LocalizationService);
  protected readonly distanceUnitLabel = this.localization.getDistanceUnitLabel();

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

  protected getPerformanceLevel = getPerformanceLevel;
  protected getPerformanceSeverity = getPerformanceSeverity;

  protected getDriverTypeSeverity(isMainDriver: boolean): Tag["severity"] {
    return isMainDriver ? "success" : "info";
  }
}
