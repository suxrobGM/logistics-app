import { CommonModule } from "@angular/common";
import { Component, computed, effect, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, cancelSubscription, getBillingPortalUrl } from "@logistics/shared/api";
import type { SubscriptionDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TagModule } from "primeng/tag";
import { TenantService, ToastService } from "@/core/services";
import { Labels, type SeverityLevel } from "@/shared/utils";

@Component({
  selector: "app-manage-subscription",
  templateUrl: "./manage-subscription.html",
  imports: [CommonModule, CardModule, ButtonModule, TagModule, ConfirmDialogModule, RouterModule],
})
export class ManageSubscriptionComponent {
  private readonly tenantService = inject(TenantService);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly subscription = signal<SubscriptionDto | null>(null);
  protected readonly truckCount = signal(0);
  protected readonly isLoading = signal(false);
  protected readonly Labels = Labels;

  protected readonly hasSubscription = computed(() => this.subscription() != null);
  protected readonly isCancelledOrPending = computed(() => {
    const sub = this.subscription();
    return sub?.status === "cancelled" || sub?.cancelAtPeriodEnd === true;
  });

  constructor() {
    effect(() => {
      const tenantData = this.tenantService.tenantData();
      this.subscription.set(tenantData?.subscription ?? null);
      this.truckCount.set(tenantData?.truckCount ?? 0);
    });
  }

  getSubStatusSeverity(): SeverityLevel {
    return Labels.subscriptionStatusSeverity(this.subscription()!);
  }

  getSubStatusLabel(): string {
    return Labels.subscriptionStatus(this.subscription()!);
  }

  confirmCancelSubscription(): void {
    const sub = this.subscription();
    if (!sub) return;

    this.toastService.confirm({
      message:
        "Are you sure you want to cancel your subscription? Your subscription will be cancelled at the end of the billing cycle.",
      header: "Cancel Subscription",
      icon: "pi pi-exclamation-triangle",
      acceptLabel: "Yes, Cancel",
      rejectLabel: "No, Keep",
      acceptButtonStyleClass: "p-button-danger",
      rejectButtonStyleClass: "p-button-success",
      acceptIcon: "pi pi-check",
      rejectIcon: "pi pi-times",
      closeOnEscape: true,
      dismissableMask: true,
      accept: async () => {
        this.isLoading.set(true);

        await this.api.invoke(cancelSubscription, {
          id: sub.id!,
          body: {},
        });

        this.toastService.showSuccess("Subscription cancelled successfully");
        this.tenantService.refetchTenantData();
        this.isLoading.set(false);
      },
    });
  }

  async openBillingPortal(): Promise<void> {
    this.isLoading.set(true);
    const result = await this.api.invoke(getBillingPortalUrl, {
      returnUrl: window.location.href,
    });

    if (result.url) {
      window.location.href = result.url;
    }
    this.isLoading.set(false);
  }

  calcTotalSubscriptionCost(): number {
    const sub = this.subscription();
    if (!sub) return 0;
    const baseFee = sub.plan?.price ?? 0;
    const perTruckFee = sub.plan?.perTruckPrice ?? 0;
    return baseFee + perTruckFee * this.truckCount();
  }
}
