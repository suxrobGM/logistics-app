import { CommonModule } from "@angular/common";
import { Component, computed, effect, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, renewSubscription } from "@logistics/shared/api";
import type { SubscriptionDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TagModule } from "primeng/tag";
import { TenantService, ToastService } from "@/core/services";
import { Labels, type SeverityLevel } from "@/shared/utils";

@Component({
  selector: "app-renew-subscription",
  templateUrl: "./renew-subscription.html",
  imports: [CommonModule, CardModule, ButtonModule, TagModule, ConfirmDialogModule, RouterModule],
})
export class RenewSubscriptionComponent {
  private readonly tenantService = inject(TenantService);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly subscription = signal<SubscriptionDto | null>(null);
  protected readonly truckCount = signal(0);
  protected readonly isLoading = signal(false);
  protected readonly Labels = Labels;

  protected readonly planName = computed(() => this.subscription()?.plan?.name ?? "Unknown");

  constructor() {
    effect(() => {
      const tenantData = this.tenantService.tenantData();
      this.subscription.set(tenantData?.subscription ?? null);
      this.truckCount.set(tenantData?.truckCount ?? 0);
    });
  }

  getSubStatusSeverity(): SeverityLevel {
    if (!this.subscription()) return "info";
    return Labels.subscriptionStatusSeverity(this.subscription()!);
  }

  getSubStatusLabel(): string {
    if (!this.subscription()) return "Unknown";
    return Labels.subscriptionStatus(this.subscription()!);
  }

  calcTotalSubscriptionCost(): number {
    const baseFee = this.subscription()?.plan?.price ?? 0;
    const perTruckFee = this.subscription()?.plan?.perTruckPrice ?? 0;
    return baseFee + perTruckFee * this.truckCount();
  }

  confirmRenewSubscription(): void {
    this.toastService.confirm({
      message: `Resume your ${this.planName()} subscription? Your billing will restart and you'll regain full access.`,
      header: "Resume Subscription",
      icon: "pi pi-refresh",
      acceptLabel: "Yes, Resume",
      rejectLabel: "Cancel",
      accept: () => this.renewSubscription(),
    });
  }

  private async renewSubscription(): Promise<void> {
    this.isLoading.set(true);

    await this.api.invoke(renewSubscription, {
      id: this.subscription()!.id!,
      body: {},
    });

    this.tenantService.refetchTenantData();
    this.toastService.showSuccess("Subscription resumed successfully");
    this.router.navigateByUrl("/subscription/manage");

    this.isLoading.set(false);
  }
}
