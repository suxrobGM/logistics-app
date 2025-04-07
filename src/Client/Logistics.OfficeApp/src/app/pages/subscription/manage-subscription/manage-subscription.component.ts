import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {DialogModule} from "primeng/dialog";
import {InputNumberModule} from "primeng/inputnumber";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {PaymentMethodDto, SubscriptionDto, SubscriptionStatus} from "@/core/models";
import {TenantService, ToastService} from "@/core/services";
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
  readonly isLoading = signal(false);
  readonly isCancelled = signal(false);
  readonly paymentMethods = signal<PaymentMethodDto[]>([]);

  constructor(
    private readonly tenantService: TenantService,
    private readonly apiService: ApiService,
    private readonly confirmationService: ConfirmationService,
    private readonly toastService: ToastService
  ) {
    const subscription = this.tenantService.getTenantData()?.subscription;

    if (!subscription) {
      throw new Error("Subscription not found");
    }

    this.subscription = subscription;
  }

  getSubStatusSeverity(): "success" | "warn" | "danger" | "info" {
    switch (this.subscription.status) {
      case SubscriptionStatus.Active:
        return "success";
      case SubscriptionStatus.Inactive:
        return "warn";
      case SubscriptionStatus.Cancelled:
        return "danger";
      default:
        return "info";
    }
  }

  getSubStatusLabel(): string {
    switch (this.subscription.status) {
      case SubscriptionStatus.Active:
        return "Active";
      case SubscriptionStatus.Inactive:
        return "Inactive";
      case SubscriptionStatus.Cancelled:
        return "Cancelled";
      default:
        return "Unknown";
    }
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

        this.apiService.cancelSubscription({id: this.subscription.id}).subscribe((result) => {
          if (result.success) {
            this.toastService.showSuccess("Subscription cancelled successfully");
            this.isCancelled.set(true);
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
}
