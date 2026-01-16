import { CommonModule } from "@angular/common";
import { Component, inject } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { DataContainer, InvoiceStatusTag } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { IconFieldModule } from "primeng/iconfield";
import { InputIconModule } from "primeng/inputicon";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { type SalaryType, salaryTypeOptions } from "@/core/api/models";
import { PayrollInvoicesListStore } from "../store/payroll-invoices-list.store";

@Component({
  selector: "app-payroll-invoices-list",
  templateUrl: "./payroll-invoices-list.html",
  providers: [PayrollInvoicesListStore],
  imports: [
    CommonModule,
    TableModule,
    CardModule,
    InputTextModule,
    RouterModule,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
    IconFieldModule,
    InputIconModule,
    DataContainer,
  ],
})
export class PayrollInvoicesListComponent {
  private readonly router = inject(Router);
  protected readonly store = inject(PayrollInvoicesListStore);

  protected onSearch(event: Event): void {
    const searchValue = (event.target as HTMLInputElement).value;
    this.store.setSearch(searchValue);
  }

  protected addInvoice(): void {
    this.router.navigate(["/invoices/payroll/add"]);
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
