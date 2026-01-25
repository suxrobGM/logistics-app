import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInvoiceById } from "@logistics/shared/api";
import type { AddressDto, InvoiceDto } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { PdfService, TenantService, ToastService } from "@/core/services";
import { InvoiceStatusTag, PaymentStatusTag } from "@/shared/components";
import { PaymentLinkDialog, RecordPaymentDialog, SendInvoiceDialog } from "../components";

@Component({
  selector: "app-load-invoice-details",
  templateUrl: "./load-invoice-details.html",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    RouterModule,
    AddressPipe,
    InvoiceStatusTag,
    PaymentStatusTag,
    DividerModule,
    TableModule,
    TagModule,
    TooltipModule,
    CurrencyPipe,
    DatePipe,
    SendInvoiceDialog,
    RecordPaymentDialog,
    PaymentLinkDialog,
  ],
})
export class LoadInvoiceDetailsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);
  private readonly pdfService = inject(PdfService);
  private readonly toastService = inject(ToastService);

  readonly invoiceId = input.required<string>();
  readonly isLoading = signal(false);
  readonly isDownloadingPdf = signal(false);
  readonly companyName = signal<string | null>(null);
  readonly companyAddress = signal<AddressDto | null>(null);
  readonly invoice = signal<InvoiceDto | null>(null);

  // Dialog visibility signals
  readonly showSendInvoiceDialog = signal(false);
  readonly showRecordPaymentDialog = signal(false);
  readonly showPaymentLinkDialog = signal(false);

  ngOnInit(): void {
    const tenantData = this.tenantService.getTenantData();
    this.companyName.set(tenantData?.companyName ?? null);
    this.companyAddress.set(tenantData?.companyAddress ?? null);
    this.fetchInvoice();
  }

  async exportToPdf(): Promise<void> {
    const invoice = this.invoice();
    if (!invoice?.id) {
      return;
    }

    this.isDownloadingPdf.set(true);
    try {
      await this.pdfService.downloadLoadInvoicePdf(invoice.id, {
        filename: `Invoice_${invoice.number}.pdf`,
      });
    } catch {
      this.toastService.showError("Failed to download invoice PDF");
    } finally {
      this.isDownloadingPdf.set(false);
    }
  }

  getOutstandingAmount(): number {
    const invoice = this.invoice();
    if (!invoice) return 0;

    const total = invoice.total?.amount ?? 0;
    const paid = invoice.payments?.reduce((sum, p) => sum + (p.amount?.amount ?? 0), 0) ?? 0;
    return Math.max(0, total - paid);
  }

  onInvoiceSent(): void {
    this.fetchInvoice();
  }

  onPaymentRecorded(): void {
    this.fetchInvoice();
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
