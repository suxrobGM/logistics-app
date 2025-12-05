import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import { Observable } from "rxjs";
import { ApiService } from "@/core/api";
import { PagedResult } from "@/core/api/models";
import { DriverReportDto } from "@/core/api/models/report/drivers-report.dto";
import { BaseTableComponent, RangeCalendar, TableQueryParams } from "@/shared/components";
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
    RangeCalendar,
    CurrencyPipe,
    DecimalPipe,
    TagModule,
  ],
})
export class DriversDetailedComponent extends BaseTableComponent<DriverReportDto> {
  private readonly apiService = inject(ApiService);

  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());

  protected override query(params: TableQueryParams): Observable<PagedResult<DriverReportDto>> {
    const orderBy = this.apiService.formatSortField(params.sortField, params.sortOrder);

    return this.apiService.reportApi.getDriversReport({
      page: params.page + 1,
      pageSize: params.size,
      orderBy: orderBy,
      search: params.search,
      startDate: this.startDate(),
      endDate: this.endDate(),
    });
  }
  protected filterByDate(): void {
    this.fetch({ page: 0, size: 10 });
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
