import { CommonModule } from "@angular/common";
import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInvoiceById, type Address, type InvoiceDto } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import { LocalizationService } from "@logistics/shared/services";
import {
  Alert,
  Badge,
  Card,
  Divider,
  Grid,
  Icon,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiDataTable,
} from "@logistics/shared/ui";
import { PdfService, TenantService, ToastService } from "@/core/services";
import { InvoiceStatusTag, PageHeader, PaymentStatusTag } from "@/shared/components";
import { PaymentLinkDialog, RecordPaymentDialog, SendInvoiceDialog } from "../components";

@Component({
  selector: "app-load-invoice-details",
  templateUrl: "./load-invoice-details.html",
  imports: [
    AddressPipe,
    Alert,
    Badge,
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DateFormatPipe,
    Divider,
    Grid,
    Icon,
    InvoiceStatusTag,
    PageHeader,
    PaymentLinkDialog,
    PaymentStatusTag,
    RecordPaymentDialog,
    RouterModule,
    SendInvoiceDialog,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiDataTable,
  ],
})
export class LoadInvoiceDetails implements OnInit {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);
  private readonly pdfService = inject(PdfService);
  private readonly toastService = inject(ToastService);
  private readonly localization = inject(LocalizationService);

  protected readonly invoiceId = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly isDownloadingPdf = signal(false);
  protected readonly companyName = signal<string | null>(null);
  protected readonly companyAddress = signal<Address | null>(null);
  protected readonly invoice = signal<InvoiceDto | null>(null);

  /** Region-aware label: "VAT" / "Sales Tax" / "Tax". */
  protected readonly taxLabel = computed(() => this.localization.getTaxLabel());

  /** True when there is any tax to display — drives the Rate / Tax columns. */
  protected readonly hasTax = computed(() => {
    const inv = this.invoice();
    if (!inv) return false;
    if (inv.taxBehavior && inv.taxBehavior !== "exclusive") {
      return true;
    }
    if ((inv.taxTotal?.amount ?? 0) > 0) return true;
    return (
      inv.lineItems?.some((li) => (li.taxAmount ?? 0) > 0 || (li.taxRatePercent ?? 0) > 0) ?? false
    );
  });

  protected readonly isReverseCharge = computed(
    () => this.invoice()?.taxBehavior === "reverse_charge",
  );

  // Dialog visibility signals
  protected readonly showSendInvoiceDialog = signal(false);
  protected readonly showRecordPaymentDialog = signal(false);
  protected readonly showPaymentLinkDialog = signal(false);

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
