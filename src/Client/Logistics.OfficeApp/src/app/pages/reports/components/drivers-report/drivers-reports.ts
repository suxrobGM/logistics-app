import {Component, inject, model, OnInit, signal} from "@angular/core";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import { DriverReportDto} from "@/core/api/models/report/drivers-report.dto";
import { BaseTableComponent, TableQueryParams, RangeCalendar } from "@/shared/components";
import { PagedResult, Result } from "@/core/api/models";
import { Observable } from "rxjs";
import { ApiService } from "@/core/api";
import { DatePickerModule } from 'primeng/datepicker';
import { DateUtils } from "@/shared/utils";
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { TagModule } from 'primeng/tag';

@Component({
  selector: "app-drivers-report",
  templateUrl: "./drivers-report.html",
  standalone: true,
  imports: [FormsModule, ButtonModule, CardModule, InputTextModule, TableModule, DatePickerModule, RangeCalendar, CurrencyPipe, DecimalPipe, TagModule],
})
export class DriversReportComponent extends BaseTableComponent<DriverReportDto>
{ 
  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());
  protected search : string = ""; 

  private readonly apiService = inject(ApiService);


  protected override query(params: TableQueryParams): Observable<PagedResult<DriverReportDto>> {
    
    const orderBy = this.apiService.formatSortField(params.sortField, params.sortOrder);

    return this.apiService.reportApi.getDriversReport({
      page: params.page + 1,
      pageSize: params.size,
      orderBy: orderBy,
      search: params.search,
      startDate: this.startDate(),
      endDate: this.endDate() || undefined,
    });
  }
  protected filterByDate() : void {
    this.fetch({page: 0, size: 10});
  }

  protected getPerformanceLevel(efficiency: number): string {
    if (efficiency >= 2.0) return 'Excellent';
    if (efficiency >= 1.5) return 'Good';
    if (efficiency >= 1.0) return 'Average';
    return 'Below Average';
  }

  protected getPerformanceSeverity(efficiency: number): string {
    if (efficiency >= 2.0) return 'success';
    if (efficiency >= 1.5) return 'info';
    if (efficiency >= 1.0) return 'warning';
    return 'danger';
  }

  protected getDriverTypeSeverity(isMainDriver: boolean): string {
    return isMainDriver ? 'success' : 'info';
  }
}

