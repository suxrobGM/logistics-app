import { DatePipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, cancelSubscription, deleteSubscription } from "@logistics/shared/api";
import { DataContainer, PageHeader, SearchInput } from "@logistics/shared/components";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { SubscriptionsListStore } from "../store/subscriptions-list.store";

@Component({
  selector: "adm-subscriptions-list",
  templateUrl: "./subscriptions-list.html",
  providers: [SubscriptionsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CardModule,
    TableModule,
    ConfirmDialogModule,
    DataContainer,
    PageHeader,
    SearchInput,
    TagModule,
    DatePipe,
  ],
})
export class SubscriptionsList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(SubscriptionsListStore);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addSubscription(): void {
    this.router.navigate(["/subscriptions/add"]);
  }

  protected confirmToCancel(id: string): void {
    this.toastService.confirm({
      message: "Are you sure you want to cancel this subscription? The subscription will be cancelled at the end of the current billing period.",
      header: "Confirm Cancel",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-warning",
      accept: () => this.cancelSub(id),
    });
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirmDelete("subscription", () => this.deleteSub(id));
  }

  private async cancelSub(id: string): Promise<void> {
    await this.api.invoke(cancelSubscription, { id, body: {} });
    this.toastService.showSuccess("The subscription has been cancelled");
    this.store.load();
  }

  private async deleteSub(id: string): Promise<void> {
    await this.api.invoke(deleteSubscription, { id });
    this.toastService.showSuccess("The subscription has been deleted successfully");
    this.store.removeItem(id);
  }

  protected getStatusSeverity(status?: string): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
    switch (status) {
      case "active":
        return "success";
      case "trialing":
        return "info";
      case "past_due":
        return "warn";
      case "cancelled":
      case "unpaid":
        return "danger";
      default:
        return "secondary";
    }
  }
}
