import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { Api, cancelSubscription$Json } from "@/core/api";
import type { SubscriptionDto } from "@/core/api/models";
import { TenantService, ToastService } from "@/core/services";
import { Labels, type SeverityLevel } from "@/shared/utils";
import { BillingHistoryComponent, PaymentMethodsCardComponent } from "../components";

@Component({
  selector: "app-manage-subscription",
  templateUrl: "./manage-subscription.html",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    TableModule,
    DialogModule,
    InputNumberModule,
    TagModule,
    ConfirmDialogModule,
    BillingHistoryComponent,
    RouterModule,
    PaymentMethodsCardComponent,
  ],
})
export class ManageSubscriptionComponent {
  private readonly tenantService = inject(TenantService);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly subscription: SubscriptionDto;
  protected readonly employeeCount: number;
  protected readonly isLoading = signal(false);
  protected readonly isCancelled = signal(false);

  constructor() {
    const tenantData = this.tenantService.getTenantData();

    if (!tenantData?.subscription) {
      throw new Error("Subscription not found");
    }

    this.subscription = tenantData.subscription;
    this.employeeCount = tenantData.employeeCount ?? 0;
  }

  getSubStatusSeverity(): SeverityLevel {
    return Labels.subscriptionStatusSeverity(this.subscription);
  }

  getSubStatusLabel(): string {
    return Labels.subscriptionStatus(this.subscription);
  }

  confirmCancelSubscription(): void {
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

        const result = await this.api.invoke(cancelSubscription$Json, {
          id: this.subscription.id!,
        });

        if (result.success) {
          this.toastService.showSuccess("Subscription cancelled successfully");
          this.isCancelled.set(true);
          this.tenantService.refetchTenantData();
          this.router.navigateByUrl("/");
        }

        this.isLoading.set(false);
      },
    });
  }

  isSubscriptionCancelled(): boolean {
    return this.subscription.status === "cancelled";
  }

  isActiveSubscription(): boolean {
    return this.subscription.status === "active";
  }

  calcTotalSubscriptionCost(subscription: SubscriptionDto): number {
    return (subscription.plan?.price ?? 0) * this.employeeCount;
  }
}
