import { CommonModule, CurrencyPipe, DatePipe, PercentPipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import {
  Api,
  approvePayrollInvoice,
  getInvoiceById,
  rejectPayrollInvoice,
  submitPayrollForApproval,
} from "@logistics/shared/api";
import type { InvoiceDto } from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TextareaModule } from "primeng/textarea";
import { TooltipModule } from "primeng/tooltip";
import { PdfService, ToastService } from "@/core/services";
import { RecordPaymentDialog } from "@/pages/invoices/components";
import { InvoiceStatusTag, PaymentStatusTag } from "@/shared/components";

@Component({
  selector: "app-payroll-invoice-details",
  templateUrl: "./payroll-invoice-details.html",
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    RouterModule,
    InvoiceStatusTag,
    PaymentStatusTag,
    DividerModule,
    TableModule,
    TagModule,
    TooltipModule,
    CurrencyPipe,
    DatePipe,
    PercentPipe,
    RecordPaymentDialog,
    DialogModule,
    TextareaModule,
  ],
})
export class PayrollInvoiceDetails implements OnInit {
  private readonly api = inject(Api);
  private readonly pdfService = inject(PdfService);
  private readonly toastService = inject(ToastService);

  protected readonly invoiceId = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly isDownloadingPdf = signal(false);
  protected readonly invoice = signal<InvoiceDto | null>(null);

  // Dialog signals
  protected readonly showRecordPaymentDialog = signal(false);
  protected readonly showRejectDialog = signal(false);
  protected readonly rejectionReason = signal("");

  // Action loading states
  protected readonly isSubmitting = signal(false);
  protected readonly isApproving = signal(false);
  protected readonly isRejecting = signal(false);

  ngOnInit(): void {
    this.fetchInvoice();
  }

  getSalaryTypeLabel(value?: string): string {
    if (!value) return "N/A";
    return salaryTypeOptions.find((opt) => opt.value === value)?.label ?? value;
  }

  getOutstandingAmount(): number {
    const invoice = this.invoice();
    if (!invoice) return 0;

    const total = invoice.total?.amount ?? 0;
    const paid = invoice.payments?.reduce((sum, p) => sum + (p.amount?.amount ?? 0), 0) ?? 0;
    return Math.max(0, total - paid);
  }

  canSubmitForApproval(): boolean {
    return this.invoice()?.status === "draft";
  }

  canApprove(): boolean {
    return this.invoice()?.status === "pending_approval";
  }

  canReject(): boolean {
    return this.invoice()?.status === "pending_approval";
  }

  canRecordPayment(): boolean {
    const status = this.invoice()?.status;
    return (status === "approved" || status === "issued") && this.getOutstandingAmount() > 0;
  }

  async submitForApproval(): Promise<void> {
    this.toastService.confirm({
      message: "Are you sure you want to submit this payroll for approval?",
      header: "Submit for Approval",
      icon: "pi pi-question-circle",
      accept: async () => {
        this.isSubmitting.set(true);
        try {
          await this.api.invoke(submitPayrollForApproval, { id: this.invoiceId() });
          this.toastService.showSuccess("Payroll submitted for approval");
          await this.fetchInvoice();
        } catch {
          this.toastService.showError("Failed to submit payroll for approval");
        } finally {
          this.isSubmitting.set(false);
        }
      },
    });
  }

  async approve(): Promise<void> {
    this.toastService.confirm({
      message: "Are you sure you want to approve this payroll?",
      header: "Approve Payroll",
      icon: "pi pi-check-circle",
      accept: async () => {
        this.isApproving.set(true);
        try {
          await this.api.invoke(approvePayrollInvoice, {
            id: this.invoiceId(),
            body: {},
          });
          this.toastService.showSuccess("Payroll approved successfully");
          await this.fetchInvoice();
        } catch {
          this.toastService.showError("Failed to approve payroll");
        } finally {
          this.isApproving.set(false);
        }
      },
    });
  }

  openRejectDialog(): void {
    this.rejectionReason.set("");
    this.showRejectDialog.set(true);
  }

  async confirmReject(): Promise<void> {
    const reason = this.rejectionReason().trim();
    if (!reason) {
      this.toastService.showError("Please provide a reason for rejection");
      return;
    }

    this.isRejecting.set(true);
    try {
      await this.api.invoke(rejectPayrollInvoice, {
        id: this.invoiceId(),
        body: { reason },
      });
      this.toastService.showSuccess("Payroll rejected");
      this.showRejectDialog.set(false);
      await this.fetchInvoice();
    } catch {
      this.toastService.showError("Failed to reject payroll");
    } finally {
      this.isRejecting.set(false);
    }
  }

  onPaymentRecorded(): void {
    this.fetchInvoice();
  }

  async downloadPayStub(): Promise<void> {
    const invoice = this.invoice();
    if (!invoice?.id) {
      return;
    }

    this.isDownloadingPdf.set(true);
    try {
      await this.pdfService.downloadPayrollPayStubPdf(invoice.id, {
        filename: `PayStub_${invoice.number}.pdf`,
      });
    } catch {
      this.toastService.showError("Failed to download pay stub");
    } finally {
      this.isDownloadingPdf.set(false);
    }
  }

  private async fetchInvoice(): Promise<void> {
    if (!this.invoiceId()) {
      return;
    }

    this.isLoading.set(true);
    const result = await this.api.invoke(getInvoiceById, { id: this.invoiceId() });
    if (result) {
      this.invoice.set(result);
    }
    this.isLoading.set(false);
  }
}
