import { Injectable, inject, signal } from "@angular/core";
import { Api, approveExpense, rejectExpense } from "@logistics/shared/api";
import { ToastService } from "@logistics/shared/services";

export interface ExpenseActionResult {
  success: boolean;
  action: "approve" | "reject";
}

@Injectable()
export class ExpenseActionsService {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  readonly showRejectDialog = signal(false);
  readonly rejectionReason = signal("");
  readonly pendingExpenseId = signal<string | null>(null);
  readonly pendingExpenseNumber = signal<string | number | null>(null);

  private onActionComplete?: (result: ExpenseActionResult) => void;

  approve(
    expenseId: string,
    expenseNumber: string | number,
    onComplete?: (result: ExpenseActionResult) => void,
  ): void {
    this.onActionComplete = onComplete;

    this.toast.confirm({
      message: `Are you sure you want to approve expense #${expenseNumber}?`,
      header: "Confirm Approval",
      icon: "pi pi-check-circle",
      acceptButtonStyleClass: "p-button-success",
      accept: async () => {
        const result = await this.api.invoke(approveExpense, { id: expenseId });
        if (result !== undefined) {
          this.toast.showSuccess(`Expense #${expenseNumber} has been approved.`, "Approved");
          this.onActionComplete?.({ success: true, action: "approve" });
        } else {
          this.onActionComplete?.({ success: false, action: "approve" });
        }
      },
      reject: () => {
        this.onActionComplete?.({ success: false, action: "approve" });
      },
    });
  }

  reject(
    expenseId: string,
    expenseNumber: string | number,
    onComplete?: (result: ExpenseActionResult) => void,
  ): void {
    this.onActionComplete = onComplete;
    this.pendingExpenseId.set(expenseId);
    this.pendingExpenseNumber.set(expenseNumber);
    this.rejectionReason.set("");
    this.showRejectDialog.set(true);
  }

  async confirmReject(): Promise<void> {
    const expenseId = this.pendingExpenseId();
    const expenseNumber = this.pendingExpenseNumber();
    if (!expenseId) return;

    const reason = this.rejectionReason().trim();
    if (!reason) {
      this.toast.showError("Please provide a reason for rejection.");
      return;
    }

    const result = await this.api.invoke(rejectExpense, {
      id: expenseId,
      body: { reason },
    });

    if (result !== undefined) {
      this.toast.showSuccess(`Expense #${expenseNumber} has been rejected.`, "Rejected");
      this.closeRejectDialog();
      this.onActionComplete?.({ success: true, action: "reject" });
    } else {
      this.onActionComplete?.({ success: false, action: "reject" });
    }
  }

  cancelReject(): void {
    this.closeRejectDialog();
  }

  private closeRejectDialog(): void {
    this.showRejectDialog.set(false);
    this.rejectionReason.set("");
    this.pendingExpenseId.set(null);
    this.pendingExpenseNumber.set(null);
  }
}
