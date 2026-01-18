import { CurrencyPipe, TitleCasePipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, deleteSubscriptionPlan } from "@logistics/shared/api";
import { DataContainer, PageHeader, SearchInput } from "@logistics/shared/components";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { PlansListStore } from "../store/plans-list.store";

@Component({
  selector: "adm-plans-list",
  templateUrl: "./plans-list.html",
  providers: [PlansListStore],
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
    CurrencyPipe,
    TitleCasePipe,
  ],
})
export class PlansList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(PlansListStore);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addPlan(): void {
    this.router.navigate(["/subscription-plans/add"]);
  }

  protected confirmToDelete(id: string): void {
    this.toastService.confirmDelete("subscription plan", () => this.deletePlan(id));
  }

  private async deletePlan(id: string): Promise<void> {
    await this.api.invoke(deleteSubscriptionPlan, { id });
    this.toastService.showSuccess("The subscription plan has been deleted successfully");
    this.store.removeItem(id);
  }

  protected formatInterval(interval?: string, count?: number): string {
    if (!interval) return "-";
    const plural = count && count > 1 ? "s" : "";
    return `${count ?? 1} ${interval}${plural}`;
  }

  protected formatTrialPeriod(trial?: string): string {
    switch (trial) {
      case "seven_days":
        return "7 days";
      case "fourteen_days":
        return "14 days";
      case "thirty_days":
        return "30 days";
      default:
        return "None";
    }
  }
}
