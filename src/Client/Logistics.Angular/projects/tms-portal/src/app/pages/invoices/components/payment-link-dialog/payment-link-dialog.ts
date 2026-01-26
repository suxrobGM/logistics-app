import { DatePipe } from "@angular/common";
import { Component, inject, input, model, signal } from "@angular/core";
import { Api, createPaymentLink, revokePaymentLink } from "@logistics/shared/api";
import type { PaymentLinkDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-payment-link-dialog",
  templateUrl: "./payment-link-dialog.html",
  imports: [
    DialogModule,
    ProgressSpinnerModule,
    ButtonModule,
    TableModule,
    TooltipModule,
    TagModule,
    DatePipe,
  ],
})
export class PaymentLinkDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly invoiceId = input.required<string>();
  public readonly paymentLinks = input<PaymentLinkDto[]>([]);
  public readonly visible = model<boolean>(false);

  protected readonly links = signal<PaymentLinkDto[]>([]);
  protected readonly isGenerating = signal(false);

  onShow(): void {
    this.links.set(this.paymentLinks() ?? []);
  }

  async generateLink(): Promise<void> {
    this.isGenerating.set(true);
    try {
      const result = await this.api.invoke(createPaymentLink, {
        id: this.invoiceId(),
        body: { expiresInDays: 30 },
      });
      this.links.update((links) => [result, ...links]);
      this.toastService.showSuccess("Payment link generated");
    } catch {
      this.toastService.showError("Failed to generate payment link");
    } finally {
      this.isGenerating.set(false);
    }
  }

  async copyToClipboard(url: string | null | undefined): Promise<void> {
    if (!url) return;
    try {
      await navigator.clipboard.writeText(url);
      this.toastService.showSuccess("Payment link copied to clipboard");
    } catch {
      this.toastService.showError("Failed to copy link");
    }
  }

  async revokeLink(id: string | undefined): Promise<void> {
    if (!id) return;
    try {
      await this.api.invoke(revokePaymentLink, {
        invoiceId: this.invoiceId(),
        linkId: id,
      });
      this.links.update((links) =>
        links.map((link) => (link.id === id ? { ...link, isActive: false } : link)),
      );
      this.toastService.showSuccess("Payment link revoked");
    } catch {
      this.toastService.showError("Failed to revoke payment link");
    }
  }

  close(): void {
    this.visible.set(false);
  }

  getLinkStatus(link: PaymentLinkDto): "success" | "warn" | "danger" {
    if (!link.isActive) return "danger";
    if (!link.isValid) return "warn";
    return "success";
  }

  getLinkStatusLabel(link: PaymentLinkDto): string {
    if (!link.isActive) return "Revoked";
    if (!link.isValid) return "Expired";
    return "Active";
  }
}
