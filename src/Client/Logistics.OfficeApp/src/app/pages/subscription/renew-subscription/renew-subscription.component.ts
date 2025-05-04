import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {Router, RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {DialogModule} from "primeng/dialog";
import {InputNumberModule} from "primeng/inputnumber";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {SubscriptionDto} from "@/core/api/models";
import {TenantService, ToastService} from "@/core/services";
import {Labels, SeverityLevel} from "@/core/utilities";
import {PaymentMethodsCardComponent} from "../components";

@Component({
  selector: "app-renew-subscription",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    TableModule,
    DialogModule,
    InputNumberModule,
    TagModule,
    ConfirmDialogModule,
    RouterModule,
    PaymentMethodsCardComponent,
  ],
  templateUrl: "./renew-subscription.component.html",
})
export class RenewSubscriptionComponent {
  readonly employeeCount = signal(0);
  readonly subscription = signal<SubscriptionDto | null>(null);
  readonly isLoading = signal(false);

  constructor(
    private readonly tenantService: TenantService,
    private readonly apiService: ApiService,
    private readonly router: Router,
    private readonly toastService: ToastService
  ) {
    const tenantData = this.tenantService.getTenantData();
    this.subscription.set(tenantData?.subscription ?? null);
    this.employeeCount.set(tenantData?.employeeCount ?? 0);

    this.tenantService.tenantDataChanged$.subscribe((tenantData) => {
      this.subscription.set(tenantData?.subscription ?? null);
      this.employeeCount.set(tenantData?.employeeCount ?? 0);
    });
  }

  getSubStatusSeverity(): SeverityLevel {
    if (!this.subscription()) {
      return "info";
    }
    return Labels.subscriptionStatusSeverity(this.subscription()!);
  }

  getSubStatusLabel(): string {
    if (!this.subscription()) {
      return "Unknown";
    }
    return Labels.subscriptionStatus(this.subscription()!);
  }

  calcTotalSubscriptionCost(): number {
    return (this.subscription()?.plan?.price ?? 0) * this.employeeCount();
  }

  confirmRenewSubscription(): void {
    this.toastService.confrimAction({
      message:
        "Are you sure you want to renew the subscription? By renewing the subscription, you agree to the terms and conditions.",
      accept: () => {
        this.renewSubscription();
      },
    });
  }

  private renewSubscription(): void {
    this.isLoading.set(true);

    this.apiService.subscriptionApi
      .renewSubscription({id: this.subscription()!.id})
      .subscribe((result) => {
        if (result.success) {
          this.tenantService.refetchTenantData();
          this.toastService.showSuccess("Subscription renewed successfully", "Renew Subscription");
          this.router.navigateByUrl("/");
        }

        this.isLoading.set(false);
      });
  }
}
