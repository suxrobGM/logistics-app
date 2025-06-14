import {CommonModule} from "@angular/common";
import { Component, OnInit, input, signal, inject } from "@angular/core";
import {RouterModule} from "@angular/router";
import {jsPDF} from "jspdf";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ApiService} from "@/core/api";
import {AddressDto, InvoiceDto} from "@/core/api/models";
import {TenantService} from "@/core/services";
import {InvoiceStatusTag} from "@/shared/components";
import {AddressPipe} from "@/shared/pipes";

@Component({
  selector: "app-load-invoice-details",
  templateUrl: "./load-invoice-details.component.html",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    RouterModule,
    AddressPipe,
    InvoiceStatusTag,
  ],
})
export class LoadInvoiceDetailsComponent implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly tenantService = inject(TenantService);

  readonly id = input.required<string>();
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
    doc.text("Invoice", middleOfPage, 10, {align: "center"});

    // Reverting to normal font for the rest of the text
    doc.setFont("helvetica", "normal");
    doc.setFontSize(12);

    doc.text(`Company: ${this.companyName}`, 10, 20);
    doc.text(`Address: ${this.companyAddress}`, 10, 30);
    doc.text(`Load Number: ${invoice.loadNumber}`, 10, 40);
    doc.text(`Date: ${invoice.createdDate.toString()}`, 10, 60);
    doc.text(`Customer Name: ${invoice.customer!.name}`, 10, 70);
    doc.text(`Invoice Status: ${invoice.status}`, 10, 90);
    doc.text(`Amount: $${invoice.total.amount}`, 10, 100);

    // Save the PDF
    doc.save(`load-invoice-${invoice.number}.pdf`);
  }

  private fetchInvoice(): void {
    if (!this.id()) {
      return;
    }

    this.isLoading.set(true);
    this.apiService.invoiceApi.getInvoice(this.id()).subscribe((result) => {
      if (result.data) {
        this.invoice.set(result.data);
      }

      this.isLoading.set(false);
    });
  }
}
