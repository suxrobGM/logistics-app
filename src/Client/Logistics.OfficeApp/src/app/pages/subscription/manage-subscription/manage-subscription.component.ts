import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {Router, RouterModule} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {DialogModule} from "primeng/dialog";
import {InputNumberModule} from "primeng/inputnumber";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {SubscriptionDto, SubscriptionStatus} from "@/core/api/models";
import {TenantService, ToastService} from "@/core/services";
import {Labels, SeverityLevel} from "@/shared/utils";
import {BillingHistoryComponent, PaymentMethodsCardComponent} from "../components";

@Component({
  selector: "app-manage-subscription",
  templateUrl: "./manage-subscription.component.html",
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
  readonly subscription: SubscriptionDto;
  readonly employeeCount: number;
  readonly isLoading = signal(false);
  readonly isCancelled = signal(false);

  constructor(
    private readonly tenantService: TenantService,
    private readonly apiService: ApiService,
    private readonly confirmationService: ConfirmationService,
    private readonly toastService: ToastService,
    private readonly router: Router
  ) {
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
    this.confirmationService.confirm({
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
      accept: () => {
        this.isLoading.set(true);

        this.apiService.subscriptionApi
          .cancelSubscription({id: this.subscription.id})
          .subscribe((result) => {
            if (result.success) {
              this.toastService.showSuccess("Subscription cancelled successfully");
              this.isCancelled.set(true);
              this.tenantService.refetchTenantData();
              this.router.navigateByUrl("/");
            }

            this.isLoading.set(false);
          });
      },
    });
  }

  isSubscriptionCancelled(): boolean {
    return this.subscription.status === SubscriptionStatus.Cancelled;
  }

  isActiveSubscription(): boolean {
    return this.subscription.status === SubscriptionStatus.Active;
  }

  calcTotalSubscriptionCost(subscription: SubscriptionDto): number {
    return (subscription.plan?.price ?? 0) * this.employeeCount;
  }
}
