import {CommonModule} from "@angular/common";
import {Component, OnInit} from "@angular/core";
import {ActivatedRoute, RouterModule} from "@angular/router";
import {jsPDF} from "jspdf";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ApiService} from "@/core/api";
import {
  AddressDto,
  InvoiceDto,
  PaymentMethodType,
  paymentMethodTypeOptions,
} from "@/core/api/models";
import {AddressPipe} from "@/core/pipes";
import {TenantService} from "@/core/services";

@Component({
  selector: "app-view-invoice",
  standalone: true,
  templateUrl: "./view-invoice.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    RouterModule,
    AddressPipe,
  ],
})
export class ViewInvoiceComponent implements OnInit {
  private id?: string;
  public isLoading = false;
  public companyName?: string;
  public companyAddress?: AddressDto;
  public invoice?: InvoiceDto;

  constructor(
    private readonly apiService: ApiService,
    private readonly tenantService: TenantService,
    private readonly route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params["id"];
    });

    const tenantData = this.tenantService.getTenantData();
    this.companyName = tenantData?.companyName;
    this.companyAddress = tenantData?.companyAddress;
    this.fetchInvoice();
  }

  getPaymentMethodDesc(enumValue?: PaymentMethodType): string {
    if (enumValue == null) {
      return "N/A";
    }
    return paymentMethodTypeOptions.find((x) => x.value === enumValue)?.label ?? "N/A";
  }

  exportToPDF() {
    if (!this.invoice) {
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
    doc.text(`Load ID: ${this.invoice.loadRef}`, 10, 40);
    doc.text(`Date: ${new Date(this.invoice.createdDate).toString()}`, 10, 60);
    doc.text(`Customer Name: ${this.invoice.customer.name}`, 10, 70);
    doc.text(`Payment Method: ${this.getPaymentMethodDesc(this.invoice.payment.method)}`, 10, 90);
    doc.text(`Amount: $${this.invoice.payment.amount}`, 10, 100);
    doc.text(`Payment Date: ${this.invoice.payment.paymentDate}`, 10, 110);

    // Save the PDF
    doc.save(`invoice-${this.invoice.loadRef}.pdf`);
  }

  private fetchInvoice() {
    if (!this.id) {
      return;
    }

    this.isLoading = true;
    this.apiService.getInvoice(this.id).subscribe((result) => {
      if (result.data) {
        this.invoice = result.data;
      }

      this.isLoading = false;
    });
  }
}
