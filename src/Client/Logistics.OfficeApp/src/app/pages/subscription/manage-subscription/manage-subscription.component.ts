import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {ConfirmationService} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {DialogModule} from "primeng/dialog";
import {InputNumberModule} from "primeng/inputnumber";
import {TableModule} from "primeng/table";
import {TagModule} from "primeng/tag";
import {SubscriptionDto, SubscriptionStatus} from "@/core/models";
import {ApiService, TenantService} from "@/core/services";
import {BillingHistoryComponent} from "../components";

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
  ],
  providers: [ConfirmationService],
})
export class ManageSubscriptionComponent {
  readonly subscription: SubscriptionDto;

  constructor(
    private readonly tenantService: TenantService,
    private readonly apiService: ApiService,
    private readonly confirmationService: ConfirmationService
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
}
