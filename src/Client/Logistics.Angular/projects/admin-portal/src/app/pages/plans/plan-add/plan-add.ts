import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, createSubscriptionPlan } from "@logistics/shared/api";
import type { CreateSubscriptionPlanCommand } from "@logistics/shared/api/models";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { PlanForm, type PlanFormValue } from "@/shared/components";

@Component({
  selector: "adm-plan-add",
  templateUrl: "./plan-add.html",
  imports: [CardModule, ButtonModule, RouterModule, PlanForm, DividerModule],
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
      description: formValue.description || null,
      price: formValue.price,
      interval: formValue.interval,
      intervalCount: formValue.intervalCount,
      trialPeriod: formValue.trialPeriod,
    };

    await this.api.invoke(createSubscriptionPlan, { body: command });
    this.toastService.showSuccess("Subscription plan has been created successfully");
    this.router.navigateByUrl("/subscription-plans");

    this.isLoading.set(false);
  }
}
