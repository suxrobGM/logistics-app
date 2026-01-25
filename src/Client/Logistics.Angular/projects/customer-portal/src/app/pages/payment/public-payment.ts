import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, effect, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ErrorState, LoadingSkeleton, ToastService } from "@logistics/shared";
import {
  Api,
  type PublicInvoiceDto,
  getPublicInvoice,
  processPublicPayment,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { InputNumberModule } from "primeng/inputnumber";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { PublicLayout } from "@/shared/layout";

@Component({
  selector: "cp-public-payment",
  templateUrl: "./public-payment.html",
  imports: [
    CurrencyPipe,
    DatePipe,
    FormsModule,
    ButtonModule,
    CardModule,
    DividerModule,
    InputNumberModule,
    TagModule,
    TableModule,
    ErrorState,
    LoadingSkeleton,
    PublicLayout,
  ],
})
export class PublicPayment {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly tenantId = input.required<string>();
  protected readonly token = input.required<string>();

  protected readonly invoice = signal<PublicInvoiceDto | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly isProcessing = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly paymentSuccess = signal(false);
  protected readonly paymentAmount = signal<number | null>(null);

  constructor() {
    effect(() => {
      const tenantId = this.tenantId();
      const token = this.token();
      if (tenantId && token) {
        this.loadInvoice(tenantId, token);
      }
    });
  }

  private async loadInvoice(tenantId: string, token: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const invoiceData = await this.api.invoke(getPublicInvoice, { tenantId, token });
      this.invoice.set(invoiceData);
      this.paymentAmount.set(invoiceData.amountDue ?? 0);
    } catch (err: unknown) {
      console.error("Failed to load invoice:", err);
      const errorMessage =
        err instanceof Error && err.message.includes("expired")
          ? "This payment link has expired or been revoked."
          : "Unable to load invoice. The link may be invalid or expired.";
      this.error.set(errorMessage);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getStatusSeverity(): "success" | "info" | "warn" | "danger" {
    const status = this.invoice()?.status;
    switch (status) {
      case "paid":
        return "success";
      case "partially_paid":
        return "info";
      case "issued":
        return "warn";
      case "cancelled":
        return "danger";
      default:
        return "info";
    }
  }

  protected getStatusLabel(): string {
    const status = this.invoice()?.status;
    switch (status) {
      case "paid":
        return "Paid";
      case "partially_paid":
        return "Partially Paid";
      case "issued":
        return "Pending";
      case "draft":
        return "Draft";
      case "cancelled":
        return "Cancelled";
      default:
        return status ?? "Unknown";
    }
  }

  protected async processPayment(): Promise<void> {
    const amount = this.paymentAmount();
    if (!amount || amount <= 0) {
      this.toastService.showError("Please enter a valid payment amount");
      return;
    }

    this.isProcessing.set(true);
    try {
      const result = await this.api.invoke(processPublicPayment, {
        tenantId: this.tenantId(),
        token: this.token(),
        body: {
          amount,
          paymentMethodId: null, // Will be set by Stripe Elements in production
        },
      });

      if (result.status === "succeeded" || result.status === "requires_action") {
        this.paymentSuccess.set(true);
        this.toastService.showSuccess("Payment initiated successfully");
      } else if (result.clientSecret) {
        // Handle Stripe payment confirmation if needed
        this.toastService.showInfo("Please complete payment confirmation");
      }
    } catch (err) {
      console.error("Payment failed:", err);
      this.toastService.showError("Payment processing failed. Please try again.");
    } finally {
      this.isProcessing.set(false);
    }
  }

  protected canPay(): boolean {
    const inv = this.invoice();
    return (inv?.canPay ?? false) && (inv?.amountDue ?? 0) > 0;
  }
}
