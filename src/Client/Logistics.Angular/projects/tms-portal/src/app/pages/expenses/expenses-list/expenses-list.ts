import { CommonModule } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, downloadExpenseReceipt, type ExpenseDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Card,
  Icon,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  UiTooltip,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { downloadBlobFile } from "@logistics/shared/utils";
import { DataContainer, PageHeader, SearchField } from "@/shared/components";
import { ExpenseStatusTag, ExpenseTypeTag } from "@/shared/components/tags";
import { getCategoryLabel, RejectExpenseDialog } from "../_components";
import { ExpenseActionsService } from "../services/expense-actions.service";
import { ExpensesListStore } from "../store/expenses-list.store";

@Component({
  selector: "app-expenses-list",
  templateUrl: "./expenses-list.html",
  providers: [ExpensesListStore, ExpenseActionsService],
  imports: [
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DataContainer,
    DateFormatPipe,
    ExpenseStatusTag,
    ExpenseTypeTag,
    Icon,
    PageHeader,
    RejectExpenseDialog,
    RouterModule,
    SearchField,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
    UiTooltip,
  ],
})
export class ExpensesListPage {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  protected readonly store = inject(ExpensesListStore);
  protected readonly expenseActions = inject(ExpenseActionsService);

  protected readonly selectedRow = signal<ExpenseDto | null>(null);

  protected readonly actionMenuItems = computed<UiMenuItem[]>(() => {
    const expense = this.selectedRow();
    const canApproveOrReject = expense?.status === "pending";

    const items: UiMenuItem[] = [
      {
        label: "View Details",
        icon: "eye",
        command: () => this.viewDetails(),
      },
      {
        label: "Edit",
        icon: "pencil",
        command: () => this.editExpense(),
      },
    ];

    if (canApproveOrReject) {
      items.push(
        { separator: true },
        {
          label: "Approve",
          icon: "check",
          command: () => this.approveExpense(),
        },
        {
          label: "Reject",
          icon: "x",
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

  protected readonly getCategoryLabel = getCategoryLabel;

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
