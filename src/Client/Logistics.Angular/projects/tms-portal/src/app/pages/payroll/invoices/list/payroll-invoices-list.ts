import { CommonModule } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import {
  Api,
  approvePayrollInvoice,
  batchApprovePayroll,
  rejectPayrollInvoice,
} from "@logistics/shared/api";
import { type InvoiceDto, type InvoiceStatus, type SalaryType } from "@logistics/shared/api";
import { invoiceStatusOptions, salaryTypeOptions } from "@logistics/shared/api/enums";
import type { SelectItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { MultiSelectModule } from "primeng/multiselect";
import { TableModule } from "primeng/table";
import { TextareaModule } from "primeng/textarea";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DataContainer, InvoiceStatusTag, LabeledField, SearchInput } from "@/shared/components";
import { PayrollInvoicesListStore } from "../../store/invoices-list.store";

@Component({
  selector: "app-payroll-invoices-list",
  templateUrl: "./payroll-invoices-list.html",
  providers: [PayrollInvoicesListStore],
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    CardModule,
    RouterModule,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
    DataContainer,
    MultiSelectModule,
    DatePickerModule,
    SearchInput,
    LabeledField,
    DialogModule,
    TextareaModule,
  ],
})
export class PayrollInvoicesList {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(PayrollInvoicesListStore);

  // Filter state
  protected readonly selectedStatuses = signal<InvoiceStatus[]>([]);
  protected readonly selectedSalaryTypes = signal<SalaryType[]>([]);
  protected readonly dateRange = signal<Date[] | null>(null);

  // Selection state
  protected readonly selectedInvoices = signal<InvoiceDto[]>([]);

  // Dialog state
  protected readonly showRejectDialog = signal(false);
  protected readonly rejectionReason = signal("");
  private invoiceToReject: InvoiceDto | null = null;

  // Loading states
  protected readonly isBatchApproving = signal(false);
  protected readonly isRejecting = signal(false);

  // Filter options
  protected readonly statusOptions: SelectItem[] = invoiceStatusOptions;
  protected readonly salaryTypeOptions: SelectItem[] = salaryTypeOptions;

  // Computed: count of active filters
  protected readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.selectedStatuses().length > 0) count++;
    if (this.selectedSalaryTypes().length > 0) count++;
    if (this.dateRange()?.length === 2) count++;
    return count;
  });

  // Computed: check if there are any selectable invoices (pending_approval status)
  protected readonly hasSelectableInvoices = computed(() => {
    return this.store.data().some((invoice) => invoice.status === "pending_approval");
  });

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected applyFilters(): void {
    const filters: Record<string, unknown> = {};

    // Status filter - support multiple statuses
    const statuses = this.selectedStatuses();
    if (statuses.length > 0) {
      filters["Status"] = statuses.join(",");
    }

    // Salary type filter - support multiple types
    const salaryTypes = this.selectedSalaryTypes();
    if (salaryTypes.length > 0) {
      filters["SalaryType"] = salaryTypes.join(",");
    }

    // Date range filter
    const range = this.dateRange();
    if (range?.length === 2) {
      filters["StartDate"] = range[0].toISOString();
      filters["EndDate"] = range[1].toISOString();
    }

    this.store.setFilters(filters);
  }

  protected clearFilters(): void {
    this.selectedStatuses.set([]);
    this.selectedSalaryTypes.set([]);
    this.dateRange.set(null);
    this.store.setFilters({});
  }

  protected addInvoice(): void {
    this.router.navigate(["/payroll/invoices/add"]);
  }

  protected clearSelection(): void {
    this.selectedInvoices.set([]);
  }

  protected onSelectionChange(invoices: InvoiceDto[]): void {
    // Filter to only include invoices that can be selected (pending_approval status)
    const selectableInvoices = invoices.filter((i) => i.status === "pending_approval");
    this.selectedInvoices.set(selectableInvoices);
  }

  protected async batchApprove(): Promise<void> {
    const selected = this.selectedInvoices();
    const pendingApproval = selected.filter((i) => i.status === "pending_approval");

    if (pendingApproval.length === 0) {
      this.toastService.showWarning("No invoices pending approval selected");
      return;
    }

    this.toastService.confirm({
      message: `Are you sure you want to approve ${pendingApproval.length} payroll invoice(s)?`,
      header: "Batch Approve",
      icon: "pi pi-check-circle",
      accept: async () => {
        this.isBatchApproving.set(true);
        try {
          await this.api.invoke(batchApprovePayroll, {
            body: {
              ids: pendingApproval.map((i) => i.id!),
            },
          });
          this.toastService.showSuccess(`Approved ${pendingApproval.length} payroll invoice(s)`);
          this.clearSelection();
          this.store.retry();
        } catch {
          this.toastService.showError("Failed to approve payroll invoices");
        } finally {
          this.isBatchApproving.set(false);
        }
      },
    });
  }

  protected approveInvoice(invoice: InvoiceDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to approve the payroll for ${invoice.employee?.fullName}?`,
      header: "Approve Payroll",
      icon: "pi pi-check-circle",
      accept: async () => {
        try {
          await this.api.invoke(approvePayrollInvoice, {
            id: invoice.id!,
            body: {},
          });
          this.toastService.showSuccess("Payroll approved successfully");
          this.store.retry();
        } catch {
          this.toastService.showError("Failed to approve payroll");
        }
      },
    });
  }

  protected openRejectDialog(invoice: InvoiceDto): void {
    this.invoiceToReject = invoice;
    this.rejectionReason.set("");
    this.showRejectDialog.set(true);
  }

  protected async confirmReject(): Promise<void> {
    const reason = this.rejectionReason().trim();
    if (!reason) {
      this.toastService.showError("Please provide a reason for rejection");
      return;
    }

    if (!this.invoiceToReject) return;

    this.isRejecting.set(true);
    try {
      await this.api.invoke(rejectPayrollInvoice, {
        id: this.invoiceToReject.id!,
        body: { reason },
      });
      this.toastService.showSuccess("Payroll rejected");
      this.showRejectDialog.set(false);
      this.invoiceToReject = null;
      this.store.retry();
    } catch {
      this.toastService.showError("Failed to reject payroll");
    } finally {
      this.isRejecting.set(false);
    }
  }

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "N/A";
  }
}
