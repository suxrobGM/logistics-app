import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  downloadExpenseReceipt,
  getExpenseById,
  type ExpenseDto,
} from "@logistics/shared/api";
import { Card, Container, Grid, Icon, Spinner, Typography, UiButton } from "@logistics/shared/ui";
import { downloadBlobFile } from "@logistics/shared/utils";
import { PageHeader } from "@/shared/components";
import {
  ExpenseDetailAuditCard,
  ExpenseDetailExtraCard,
  ExpenseDetailInfoCard,
  RejectExpenseDialog,
} from "../components";
import { ExpenseActionsService } from "../services/expense-actions.service";

@Component({
  selector: "app-expense-details",
  templateUrl: "./expense-details.html",
  providers: [ExpenseActionsService],
  imports: [
    Card,
    Container,
    ExpenseDetailAuditCard,
    ExpenseDetailExtraCard,
    ExpenseDetailInfoCard,
    Grid,
    Icon,
    PageHeader,
    RejectExpenseDialog,
    RouterModule,
    Spinner,
    Typography,
    UiButton,
  ],
})
export class ExpenseDetailsPage implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly expenseActions = inject(ExpenseActionsService);

  protected readonly id = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly expense = signal<ExpenseDto | null>(null);

  ngOnInit(): void {
    this.fetchExpense();
  }

  canApproveOrReject(): boolean {
    return this.expense()?.status === "pending";
  }

  onApprove(): void {
    const e = this.expense();
    if (!e?.id) return;

    this.expenseActions.approve(e.id, e.number!, (result) => {
      if (result.success) this.fetchExpense();
    });
  }

  onReject(): void {
    const e = this.expense();
    if (!e?.id) return;

    this.expenseActions.reject(e.id, e.number!, (result) => {
      if (result.success) this.fetchExpense();
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
    if (!this.id()) return;

    this.isLoading.set(true);
    const result = await this.api.invoke(getExpenseById, { id: this.id() });
    if (result) this.expense.set(result);
    this.isLoading.set(false);
  }
}
