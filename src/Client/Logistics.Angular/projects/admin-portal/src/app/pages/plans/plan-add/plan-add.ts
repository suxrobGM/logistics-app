import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import {
  Api,
  createSubscriptionPlan,
  type CreateSubscriptionPlanCommand,
} from "@logistics/shared/api";
import { Card, PageHeader } from "@logistics/shared/ui";
import { PlanForm, type PlanFormValue } from "@/shared/components";

@Component({
  selector: "adm-plan-add",
  templateUrl: "./plan-add.html",
  imports: [Card, PageHeader, PlanForm, RouterModule],
})
export class PlanAdd {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal<boolean>(false);

  protected async onSave(formValue: PlanFormValue): Promise<void> {
    this.isLoading.set(true);

    const command: CreateSubscriptionPlanCommand = {
      name: formValue.name,
      description: formValue.description,
      tier: formValue.tier,
      price: formValue.price,
      perTruckPrice: formValue.perTruckPrice,
      maxTrucks: formValue.maxTrucks,
      weeklyAiRequestQuota: formValue.weeklyAiRequestQuota,
      interval: formValue.interval,
      intervalCount: formValue.intervalCount,
    };

    await this.api.invoke(createSubscriptionPlan, { body: command });
    this.toastService.showSuccess("Subscription plan has been created successfully");
    this.router.navigateByUrl("/subscription-plans");

    this.isLoading.set(false);
  }
}
