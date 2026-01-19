import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import type { ExpenseDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import type { MenuItem } from "primeng/api";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { ExpensesListStore } from "../store/expenses-list.store";

@Component({
  selector: "app-expenses-list",
  templateUrl: "./expenses-list.html",
  providers: [ExpensesListStore],
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    TableModule,
    ButtonModule,
    TagModule,
    MenuModule,
    TooltipModule,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class ExpensesListPage {
  private readonly router = inject(Router);
  readonly store = inject(ExpensesListStore);
  readonly selectedRow = signal<ExpenseDto | null>(null);
  readonly actionMenuItems: MenuItem[] = [
    {
      label: "View Details",
      icon: "pi pi-eye",
      command: () => this.viewDetails(),
    },
    {
      label: "Edit",
      icon: "pi pi-pencil",
      command: () => this.editExpense(),
    },
    {
      separator: true,
    },
    {
      label: "Approve",
      icon: "pi pi-check",
      command: () => this.approveExpense(),
    },
    {
      label: "Reject",
      icon: "pi pi-times",
      command: () => this.rejectExpense(),
    },
  ];

  onSearch(search: string): void {
    this.store.setSearch(search);
  }

  getTypeLabel(type: string): string {
    switch (type) {
      case "Company":
        return "Company";
      case "Truck":
        return "Truck";
      case "BodyShop":
        return "Body Shop";
      default:
        return type;
    }
  }

  getTypeSeverity(type: string): "info" | "success" | "warn" | "danger" | "secondary" | "contrast" {
    switch (type) {
      case "Company":
        return "info";
      case "Truck":
        return "success";
      case "BodyShop":
        return "warn";
      default:
        return "secondary";
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case "Pending":
        return "Pending";
      case "Approved":
        return "Approved";
      case "Rejected":
        return "Rejected";
      case "Paid":
        return "Paid";
      default:
        return status;
    }
  }

  getStatusSeverity(status: string): "info" | "success" | "warn" | "danger" | "secondary" | "contrast" {
    switch (status) {
      case "Pending":
        return "warn";
      case "Approved":
        return "success";
      case "Rejected":
        return "danger";
      case "Paid":
        return "info";
      default:
        return "secondary";
    }
  }

  getCategoryLabel(expense: ExpenseDto): string {
    if (expense.companyCategory) {
      return expense.companyCategory;
    }
    if (expense.truckCategory) {
      return expense.truckCategory;
    }
    return "N/A";
  }

  private viewDetails(): void {
    const expense = this.selectedRow();
    if (expense) {
      this.router.navigate(["/expenses", expense.id]);
    }
  }

  private editExpense(): void {
    const expense = this.selectedRow();
    if (expense) {
      this.router.navigate(["/expenses", expense.id, "edit"]);
    }
  }

  private approveExpense(): void {
    // TODO: Implement approve dialog
    console.log("Approve expense", this.selectedRow());
  }

  private rejectExpense(): void {
    // TODO: Implement reject dialog
    console.log("Reject expense", this.selectedRow());
  }
}
