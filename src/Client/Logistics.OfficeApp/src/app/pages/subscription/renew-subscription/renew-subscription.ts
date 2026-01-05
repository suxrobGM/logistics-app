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
import { Api, renewSubscription$Json } from "@/core/api";
import { SubscriptionDto } from "@/core/api/models";
import { TenantService, ToastService } from "@/core/services";
import { Labels, SeverityLevel } from "@/shared/utils";
import { PaymentMethodsCardComponent } from "../components";

@Component({
  selector: "app-renew-subscription",
  templateUrl: "./renew-subscription.html",
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
})
export class RenewSubscriptionComponent {
  private readonly tenantService = inject(TenantService);
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly employeeCount = signal(0);
  protected readonly subscription = signal<SubscriptionDto | null>(null);
  protected readonly isLoading = signal(false);

  constructor() {
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
    this.toastService.confirm({
      message:
        "Are you sure you want to renew the subscription? By renewing the subscription, you agree to the terms and conditions.",
      accept: () => {
        this.renewSubscription();
      },
    });
  }

  private async renewSubscription(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(renewSubscription$Json, {
      id: this.subscription()!.id!,
    });

    if (result.success) {
      this.tenantService.refetchTenantData();
      this.toastService.showSuccess("Subscription renewed successfully", "Renew Subscription");
      this.router.navigateByUrl("/");
    }

    this.isLoading.set(false);
  }
}
