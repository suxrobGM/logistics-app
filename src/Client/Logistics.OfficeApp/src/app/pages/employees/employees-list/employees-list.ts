import { CurrencyPipe, DatePipe, PercentPipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { type SalaryType, salaryTypeOptions } from "@/core/api/models";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { EmployeesListStore } from "../store/employees-list.store";

@Component({
  selector: "app-employees-list",
  templateUrl: "./employees-list.html",
  providers: [EmployeesListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    DatePipe,
    PercentPipe,
    CurrencyPipe,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class EmployeeListComponent {
  private readonly router = inject(Router);
  protected readonly store = inject(EmployeesListStore);

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected addEmployee(): void {
    this.router.navigate(["/employees/add"]);
  }

  protected getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
