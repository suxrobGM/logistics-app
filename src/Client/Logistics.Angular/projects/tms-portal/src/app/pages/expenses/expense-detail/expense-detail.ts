import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, downloadExpenseReceipt, getExpenseById } from "@logistics/shared/api";
import type { ExpenseDto } from "@logistics/shared/api";
import { downloadBlobFile } from "@logistics/shared/utils";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { PageHeader } from "@/shared/components";
import { ExpenseStatusTag, ExpenseTypeTag } from "@/shared/components/tags";
import { RejectExpenseDialog } from "../components";
import { ExpenseActionsService } from "../services/expense-actions.service";

@Component({
  selector: "app-expense-detail",
  templateUrl: "./expense-detail.html",
  providers: [ExpenseActionsService],
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    RouterModule,
    DividerModule,
    PageHeader,
    ExpenseStatusTag,
    ExpenseTypeTag,
    RejectExpenseDialog,
  ],
})
export class ExpenseDetailPage implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly expenseActions = inject(ExpenseActionsService);

  protected readonly id = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly expense = signal<ExpenseDto | null>(null);

  ngOnInit(): void {
    this.fetchExpense();
  }

  getCategoryLabel(): string {
    const e = this.expense();
    if (!e) return "N/A";
    return e.companyCategory ?? e.truckCategory ?? "N/A";
  }

  canApproveOrReject(): boolean {
    return this.expense()?.status === "pending";
  }

  onApprove(): void {
    const e = this.expense();
    if (!e?.id) return;

    this.expenseActions.approve(e.id, e.number!, (result) => {
      if (result.success) {
        this.fetchExpense();
      }
    });
  }

  onReject(): void {
    const e = this.expense();
    if (!e?.id) return;

    this.expenseActions.reject(e.id, e.number!, (result) => {
      if (result.success) {
        this.fetchExpense();
      }
    });
  }

  onEdit(): void {
    this.router.navigate(["/expenses", this.id(), "edit"]);
  }

  async viewReceipt(): Promise<void> {
    const e = this.expense();
    if (!e?.id || !e.receiptBlobPath) return;

    const blob = await this.api.invoke(downloadExpenseReceipt, { id: e.id });
    if (blob) {
      const fileName = e.receiptBlobPath.split("/").pop() ?? "receipt";
      downloadBlobFile(blob, fileName);
    }
  }

  private async fetchExpense(): Promise<void> {
    if (!this.id()) {
      return;
    }

    this.isLoading.set(true);
    const result = await this.api.invoke(getExpenseById, { id: this.id() });
    if (result) {
      this.expense.set(result);
    }
    this.isLoading.set(false);
  }
}
