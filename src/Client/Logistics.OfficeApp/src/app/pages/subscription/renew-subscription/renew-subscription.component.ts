import {CommonModule} from "@angular/common";
import {Component, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {DialogModule} from "primeng/dialog";
import {InputNumberModule} from "primeng/inputnumber";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {ApiService} from "@/core/api";
import {SubscriptionDto} from "@/core/api/models";
import {TenantService} from "@/core/services";
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
  readonly subscription: SubscriptionDto;
  readonly isLoading = signal(false);
  readonly isCancelled = signal(false);

  constructor(
    private readonly tenantService: TenantService,
    private readonly apiService: ApiService
  ) {
    const subscription = this.tenantService.getTenantData()?.subscription;

    if (!subscription) {
      throw new Error("Subscription not found");
    }

    this.subscription = subscription;
  }

  getSubStatusSeverity(): SeverityLevel {
    return Labels.subscriptionStatusSeverity(this.subscription);
  }

  getSubStatusLabel(): string {
    return Labels.subscriptionStatus(this.subscription);
  }
}
