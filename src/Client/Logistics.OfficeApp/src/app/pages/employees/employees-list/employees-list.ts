import { CommonModule, CurrencyPipe, DatePipe, PercentPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { SharedModule } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { type TableLazyLoadEvent, TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, formatSortField, getEmployees$Json } from "@/core/api";
import { type EmployeeDto, type SalaryType, salaryTypeOptions } from "@/core/api/models";

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
export class EmployeeListComponent {
  private readonly api = inject(Api);

  protected readonly data = signal<EmployeeDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);

  protected async onLazyLoad(event: TableLazyLoadEvent): Promise<void> {
    this.isLoading.set(true);
    const rows = event.rows ?? 10;
    const page = (event.first ?? 0) / rows;
    const orderBy = formatSortField(event.sortField as string, event.sortOrder);

    const result = await this.api.invoke(getEmployees$Json, {
      Page: page + 1,
      PageSize: rows,
      OrderBy: orderBy,
    });

    if (result.data) {
      this.data.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
      this.first.set(page * rows);
    }

    this.isLoading.set(false);
  }

  protected async onSearch(event: Event): Promise<void> {
    this.isLoading.set(true);
    const value = (event.target as HTMLInputElement).value;

    const result = await this.api.invoke(getEmployees$Json, {
      Search: value,
      Page: 1,
      PageSize: 10,
    });

    if (result.data) {
      this.data.set(result.data);
      this.totalRecords.set(result.totalItems ?? 0);
      this.first.set(0);
    }

    this.isLoading.set(false);
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
