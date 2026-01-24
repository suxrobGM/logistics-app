import { CommonModule } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInvoiceById } from "@logistics/shared/api";
import type { AddressDto, InvoiceDto } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import { jsPDF } from "jspdf";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TenantService } from "@/core/services";
import { InvoiceStatusTag } from "@/shared/components";

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
    DividerModule,
  ],
})
export class LoadInvoiceDetailsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);

  readonly invoiceId = input.required<string>();
  readonly isLoading = signal(false);
  readonly companyName = signal<string | null>(null);
  readonly companyAddress = signal<AddressDto | null>(null);
  readonly invoice = signal<InvoiceDto | null>(null);

  ngOnInit(): void {
    const tenantData = this.tenantService.getTenantData();
    this.companyName.set(tenantData?.companyName ?? null);
    this.companyAddress.set(tenantData?.companyAddress ?? null);
    this.fetchInvoice();
  }

  exportToPdf(): void {
    const invoice = this.invoice();
    if (!invoice) {
      return;
    }

    const doc = new jsPDF();
    doc.setFont("helvetica", "bold");

    // Adding 'Invoice' text centered
    const middleOfPage = doc.internal.pageSize.getWidth() / 2;
    doc.setFontSize(18); // Larger font size
    doc.text("Invoice", middleOfPage, 10, { align: "center" });

    // Reverting to normal font for the rest of the text
    doc.setFont("helvetica", "normal");
    doc.setFontSize(12);

    doc.text(`Company: ${this.companyName()}`, 10, 20);
    doc.text(`Address: ${this.companyAddress()}`, 10, 30);
    doc.text(`Load Number: ${invoice.loadNumber}`, 10, 40);
    doc.text(`Date: ${invoice.createdDate?.toString() ?? "N/A"}`, 10, 60);
    doc.text(`Customer Name: ${invoice.customer?.name ?? "N/A"}`, 10, 70);
    doc.text(`Invoice Status: ${invoice.status}`, 10, 90);
    doc.text(`Amount: $${invoice.total?.amount ?? 0}`, 10, 100);

    // Save the PDF
    doc.save(`load-invoice-${invoice.number}.pdf`);
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
