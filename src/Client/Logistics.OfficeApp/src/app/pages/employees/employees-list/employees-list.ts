import { CurrencyPipe, DatePipe, PercentPipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { type SalaryType, salaryTypeOptions } from "@/core/api/models";
import { DataContainer } from "@/shared/components";
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
    InputTextModule,
    DatePipe,
    PercentPipe,
    CurrencyPipe,
    IconFieldModule,
    InputIconModule,
    DataContainer,
  ],
})
export class EmployeeListComponent {
  private readonly router = inject(Router);
  protected readonly store = inject(EmployeesListStore);

  protected onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.store.setSearch(value);
  }

  protected addEmployee(): void {
    this.router.navigate(["/employees/add"]);
  }

  protected getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
