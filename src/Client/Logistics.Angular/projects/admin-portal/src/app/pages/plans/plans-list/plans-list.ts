import { CurrencyPipe, TitleCasePipe } from "@angular/common";
import { Component, computed, inject, signal, viewChild } from "@angular/core";
import { Router } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, deleteSubscriptionPlan, type PlanTier } from "@logistics/shared/api";
import {
  Badge,
  Card,
  DataContainer,
  PageHeader,
  SearchField,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  type UiBadgeIntent,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { PlansListStore } from "../store/plans-list.store";

@Component({
  selector: "adm-plans-list",
  templateUrl: "./plans-list.html",
  providers: [PlansListStore],
  imports: [
    Badge,
    Card,
    CurrencyPipe,
    DataContainer,
    PageHeader,
    SearchField,
    TitleCasePipe,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
  ],
})
export class PlansList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(PlansListStore);

  private readonly actionMenu = viewChild<UiMenu>("actionMenu");
  private readonly selectedPlan = signal<{ id?: string; name?: string } | null>(null);

  protected readonly actionMenuItems = computed<UiMenuItem[]>(() => {
    const plan = this.selectedPlan();
    return [
      {
        label: "Edit",
        icon: "square-pen",
        command: () => this.router.navigate(["/subscription-plans", plan!.id, "edit"]),
      },
      { separator: true },
      {
        label: "Delete",
        icon: "trash",
        variant: "destructive",
        command: () => this.confirmToDelete(plan!.id!),
      },
    ];
  });

  protected openActionMenu(event: Event, plan: { id?: string; name?: string }): void {
    this.selectedPlan.set(plan);
    this.actionMenu()?.toggle(event);
  }

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

  protected formatTier(tier?: PlanTier): string {
    switch (tier) {
      case "starter":
        return "Starter";
      case "professional":
        return "Professional";
      case "enterprise":
        return "Enterprise";
      default:
        return "-";
    }
  }

  protected tierSeverity(tier?: PlanTier): UiBadgeIntent {
    switch (tier) {
      case "starter":
        return "info";
      case "professional":
        return "warn";
      case "enterprise":
        return "success";
      default:
        return "secondary";
    }
  }
}
