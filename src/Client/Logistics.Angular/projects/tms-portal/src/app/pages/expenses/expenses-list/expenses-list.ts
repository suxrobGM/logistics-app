import { CommonModule } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, downloadExpenseReceipt } from "@logistics/shared/api";
import type { ExpenseDto } from "@logistics/shared/api/models";
import { downloadBlobFile } from "@logistics/shared/utils";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { ExpenseStatusTag, ExpenseTypeTag } from "@/shared/components/tags";
import { RejectExpenseDialog } from "../components";
import { ExpenseActionsService } from "../services/expense-actions.service";
import { ExpensesListStore } from "../store/expenses-list.store";

@Component({
  selector: "app-expenses-list",
  templateUrl: "./expenses-list.html",
  providers: [ExpensesListStore, ExpenseActionsService],
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    TableModule,
    ButtonModule,
    MenuModule,
    TooltipModule,
    DataContainer,
    PageHeader,
    SearchInput,
    ExpenseStatusTag,
    ExpenseTypeTag,
    RejectExpenseDialog,
  ],
})
export class ExpensesListPage {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  protected readonly store = inject(ExpensesListStore);
  protected readonly expenseActions = inject(ExpenseActionsService);

  protected readonly selectedRow = signal<ExpenseDto | null>(null);

  protected readonly actionMenuItems = computed<MenuItem[]>(() => {
    const expense = this.selectedRow();
    const canApproveOrReject = expense?.status === "pending";

    const items: MenuItem[] = [
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
    ];

    if (canApproveOrReject) {
      items.push(
        { separator: true },
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
      );
    }

    return items;
  });

  openActionMenu(
    expense: ExpenseDto,
    menu: { toggle: (event: Event) => void },
    event: Event,
  ): void {
    this.selectedRow.set(expense);
    menu.toggle(event);
  }

  onSearch(search: string): void {
    this.store.setSearch(search);
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
    const expense = this.selectedRow();
    if (!expense?.id) return;

    this.expenseActions.approve(expense.id, expense.number!, (result) => {
      if (result.success) {
        this.store.retry();
      }
    });
  }

  private rejectExpense(): void {
    const expense = this.selectedRow();
    if (!expense?.id) return;

    this.expenseActions.reject(expense.id, expense.number!, (result) => {
      if (result.success) {
        this.store.retry();
      }
    });
  }

  async viewReceipt(expense: ExpenseDto, event: Event): Promise<void> {
    event.stopPropagation();
    if (!expense.id || !expense.receiptBlobPath) return;

    const blob = await this.api.invoke(downloadExpenseReceipt, { id: expense.id });
    if (blob) {
      const fileName = expense.receiptBlobPath.split("/").pop() ?? "receipt";
      downloadBlobFile(blob, fileName);
    }
  }
}
