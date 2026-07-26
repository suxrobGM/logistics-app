import { CommonModule } from "@angular/common";
import { Component, computed, effect, inject, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import {
  Api,
  cancelSubscription,
  getBillingPortalUrl,
  type SubscriptionDto,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { TenantService, ToastService } from "@/core/services";
import { Labels } from "@/shared/utils";

/**
 * Plan overview, pricing breakdown and subscription actions. Rendered by the Billing settings tab;
 * links out to `/subscription/plans` and `/subscription/renew`, which stay standalone routes.
 */
@Component({
  selector: "app-subscription-card",
  templateUrl: "./subscription-card.html",
  imports: [
    Badge,
    Card,
    CommonModule,
    Grid,
    Icon,
    RouterModule,
    Stack,
    Surface,
    Typography,
    UiButton,
  ],
})
export class SubscriptionCard {
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

  protected getSubStatusSeverity(): UiBadgeIntent {
    return Labels.subscriptionStatusSeverity(this.subscription()!);
  }

  protected getSubStatusLabel(): string {
    return Labels.subscriptionStatus(this.subscription()!);
  }

  protected confirmCancelSubscription(): void {
    const sub = this.subscription();
    if (!sub) return;

    this.toastService.confirm({
      message:
        "Are you sure you want to cancel your subscription? Your subscription will be cancelled at the end of the billing cycle.",
      header: "Cancel Subscription",
      icon: "warning",
      acceptLabel: "Yes, Cancel",
      rejectLabel: "No, Keep",
      severity: "danger",
      rejectSeverity: "success",
      acceptIcon: "check",
      rejectIcon: "close",
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

  protected async openBillingPortal(): Promise<void> {
    this.isLoading.set(true);
    const result = await this.api.invoke(getBillingPortalUrl, {
      returnUrl: window.location.href,
    });

    if (result.url) {
      window.location.href = result.url;
    }
    this.isLoading.set(false);
  }

  protected calcTotalSubscriptionCost(): number {
    const sub = this.subscription();
    if (!sub) return 0;
    const baseFee = sub.plan?.price ?? 0;
    const perTruckFee = sub.plan?.perTruckPrice ?? 0;
    return baseFee + perTruckFee * this.truckCount();
  }
}
