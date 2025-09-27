import { CommonModule, CurrencyPipe, DatePipe, PercentPipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { SharedModule } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Observable } from "rxjs";
import { ApiService } from "@/core/api";
import { EmployeeDto, PagedResult, SalaryType, salaryTypeOptions } from "@/core/api/models";
import { BaseTableComponent, TableQueryParams } from "@/shared/components";

@Component({
  selector: "app-employees-list",
  templateUrl: "./employees-list.html",
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    SharedModule,
    InputTextModule,
    DatePipe,
    PercentPipe,
    CurrencyPipe,
    IconFieldModule,
    InputIconModule,
  ],
})
export class EmployeeListComponent extends BaseTableComponent<EmployeeDto> {
  private readonly apiService = inject(ApiService);

  protected override query(params: TableQueryParams): Observable<PagedResult<EmployeeDto>> {
    const orderBy = this.apiService.formatSortField(params.sortField, params.sortOrder);

    return this.apiService.getEmployees({
      page: params.page + 1,
      pageSize: params.size,
      orderBy: orderBy,
      search: params.search,
    });
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
