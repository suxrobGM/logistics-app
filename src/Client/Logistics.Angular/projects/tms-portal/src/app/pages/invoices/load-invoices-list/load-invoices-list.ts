import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import type { InvoiceDto, InvoiceStatus } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { SelectButtonModule } from "primeng/selectbutton";
import { TableModule } from "primeng/table";
import { ToolbarModule } from "primeng/toolbar";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DataContainer, InvoiceStatusTag } from "@/shared/components";
import { SendInvoiceDialog } from "../components";
import { LoadInvoicesListStore } from "../store/load-invoices-list.store";

interface StatusOption {
  label: string;
  value: InvoiceStatus | "all";
}

@Component({
  selector: "app-load-invoices-list",
  templateUrl: "./load-invoices-list.html",
  providers: [LoadInvoicesListStore],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardModule,
    TableModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
    DataContainer,
    CheckboxModule,
    SelectButtonModule,
    ToolbarModule,
    SendInvoiceDialog,
  ],
})
export class LoadInvoicesListComponent implements OnInit {
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(LoadInvoicesListStore);
  protected readonly loadId = input<string>();

  protected readonly selectedInvoices = signal<InvoiceDto[]>([]);
  protected readonly statusFilter = signal<InvoiceStatus | "all">("all");
  protected readonly showSendInvoiceDialog = signal(false);
  protected readonly selectedInvoiceId = signal<string | null>(null);

  protected readonly statusOptions: StatusOption[] = [
    { label: "All", value: "all" },
    { label: "Draft", value: "draft" },
    { label: "Issued", value: "issued" },
    { label: "Partially Paid", value: "partially_paid" },
    { label: "Paid", value: "paid" },
  ];

  ngOnInit(): void {
    const id = this.loadId();
    if (id) {
      this.store.setFilters({ LoadId: id });
    }
  }

  onSelectionChange(invoices: InvoiceDto[]): void {
    this.selectedInvoices.set(invoices);
  }

  clearSelection(): void {
    this.selectedInvoices.set([]);
  }

  openSendDialog(invoice: InvoiceDto): void {
    this.selectedInvoiceId.set(invoice.id ?? null);
    this.showSendInvoiceDialog.set(true);
  }

  onInvoiceSent(): void {
    this.toastService.showSuccess("Invoice sent successfully");
  }

  isOverdue(invoice: InvoiceDto): boolean {
    if (!invoice.dueDate || invoice.status === "paid") return false;
    return new Date(invoice.dueDate) < new Date();
  }

  getFilteredData(): InvoiceDto[] {
    const data = this.store.data();
    const status = this.statusFilter();
    if (status === "all") return data;
    return data.filter((inv) => inv.status === status);
  }
}
