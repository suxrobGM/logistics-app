import { Component, inject, type OnInit, signal } from "@angular/core";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { Api, deleteSubscriptionPlan, getSubscriptionPlanById, updateSubscriptionPlan } from "@logistics/shared/api";
import type { SubscriptionPlanDto, UpdateSubscriptionPlanCommand } from "@logistics/shared/api/models";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { PlanForm, type PlanFormValue } from "@/shared/components";

@Component({
  selector: "adm-plan-edit",
  templateUrl: "./plan-edit.html",
  imports: [CardModule, ButtonModule, RouterModule, PlanForm, DividerModule, SkeletonModule],
})
export class PlanEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isFetching = signal<boolean>(true);
  protected readonly plan = signal<SubscriptionPlanDto | null>(null);

  ngOnInit(): void {
    this.fetchPlan();
  }

  private async fetchPlan(): Promise<void> {
    const id = this.route.snapshot.paramMap.get("id");
    if (!id) {
      this.router.navigateByUrl("/subscription-plans");
      return;
    }

    this.isFetching.set(true);
    const plan = await this.api.invoke(getSubscriptionPlanById, { id });

    if (!plan) {
      this.toastService.showError("Subscription plan not found");
      this.router.navigateByUrl("/subscription-plans");
      return;
    }

    this.plan.set(plan);
    this.isFetching.set(false);
  }

  protected getInitialValue(): Partial<PlanFormValue> | null {
    const plan = this.plan();
    if (!plan) return null;

    return {
      name: plan.name ?? "",
      description: plan.description ?? "",
      price: plan.price ?? 0,
      interval: plan.interval ?? "month",
      intervalCount: plan.intervalCount ?? 1,
      trialPeriod: plan.trialPeriod ?? "none",
    };
  }

  protected async onSave(formValue: PlanFormValue): Promise<void> {
    const plan = this.plan();
    if (!plan) return;

    this.isLoading.set(true);

    const command: UpdateSubscriptionPlanCommand = {
      id: plan.id!,
      name: formValue.name,
      description: formValue.description || null,
      price: formValue.price,
      interval: formValue.interval,
      intervalCount: formValue.intervalCount,
      trialPeriod: formValue.trialPeriod,
    };

    await this.api.invoke(updateSubscriptionPlan, { id: plan.id!, body: command });
    this.toastService.showSuccess("Subscription plan has been updated successfully");
    this.router.navigateByUrl("/subscription-plans");

    this.isLoading.set(false);
  }

  protected async onRemove(): Promise<void> {
    const plan = this.plan();
    if (!plan) return;

    await this.api.invoke(deleteSubscriptionPlan, { id: plan.id! });
    this.toastService.showSuccess("Subscription plan has been deleted successfully");
    this.router.navigateByUrl("/subscription-plans");
  }
}
