import { DatePipe } from "@angular/common";
import { Component, computed, effect, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ErrorState, LoadingSkeleton, ToastService } from "@logistics/shared";
import {
  Api,
  createPublicCheckoutSession,
  getPublicInvoice,
  type PublicInvoiceDto,
} from "@logistics/shared/api";
import { Alert, Grid, Icon, Stack, Surface, Typography } from "@logistics/shared/components";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
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
    CurrencyFormatPipe,
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
    Alert,
    Grid,
    Icon,
    Stack,
    Surface,
    Typography,
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

  /** Currency reported by the invoice — drives the currency input + formatting. */
  protected readonly currency = computed(() => this.invoice()?.total?.currency ?? "USD");

  /** True when the invoice carries any tax — drives the Rate% / Tax columns + breakdown row. */
  protected readonly hasTax = computed(() => {
    const inv = this.invoice();
    if (!inv) return false;
    if (inv.taxBehavior && inv.taxBehavior !== "exclusive") return true;
    if ((inv.taxTotal?.amount ?? 0) > 0) return true;
    return (
      inv.lineItems?.some((li) => (li.taxAmount ?? 0) > 0 || (li.taxRatePercent ?? 0) > 0) ?? false
    );
  });

  protected readonly isReverseCharge = computed(
    () => this.invoice()?.taxBehavior === "reverse_charge",
  );

  /**
   * Region-aware label for the tax column / row. The public DTO doesn't carry the
   * tenant's region, so default to "Tax"; if the breakdown carries a recognizable
   * tax_type it could be made smarter later.
   */
  protected readonly taxLabel = computed(() => "Tax");

  constructor() {
    effect(() => {
      const tenantId = this.tenantId();
      const token = this.token();
      if (tenantId && token) {
        this.loadInvoice(tenantId, token);
      }
    });

    // Detect Stripe Checkout return — the success URL is configured to point back here with
    // ?paid=1, so flip into the success state without an extra round-trip to the API.
    effect(() => {
      const params = new URLSearchParams(window.location.search);
      if (params.get("paid") === "1") {
        this.paymentSuccess.set(true);
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
      const baseUrl = window.location.origin + window.location.pathname;
      const result = await this.api.invoke(createPublicCheckoutSession, {
        tenantId: this.tenantId(),
        token: this.token(),
        body: {
          amount,
          successUrl: `${baseUrl}?paid=1`,
          cancelUrl: baseUrl,
        },
      });

      if (!result.url) {
        this.toastService.showError("Could not start payment session.");
        return;
      }

      // Redirect to Stripe's hosted Checkout page; Stripe will return the customer to
      // successUrl on completion or cancelUrl on cancel.
      window.location.href = result.url;
    } catch (err) {
      console.error("Payment failed:", err);
      this.toastService.showError("Payment processing failed. Please try again.");
      this.isProcessing.set(false);
    }
  }

  protected canPay(): boolean {
    const inv = this.invoice();
    return (inv?.canPay ?? false) && (inv?.amountDue ?? 0) > 0;
  }
}
