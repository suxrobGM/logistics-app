import { CurrencyPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { Api, getSubscriptionPlans$Json } from "@/core/api";
import type { SubscriptionPlanDto } from "@/core/api/models";
import { TenantService } from "@/core/services";

@Component({
  selector: "app-view-plans",
  templateUrl: "./view-plans.html",
  imports: [CardModule, CurrencyPipe, ButtonModule],
})
export class ViewPlansComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);

  protected readonly subscriptionPlans = signal<SubscriptionPlanDto[]>([]);

  async ngOnInit(): Promise<void> {
    const result = await this.api.invoke(getSubscriptionPlans$Json, {});
    if (result.items) {
      this.subscriptionPlans.set(result.items);
    }
  }

  protected isCurrentPlan(plan: SubscriptionPlanDto): boolean {
    const currentPlanId = this.tenantService.getTenantData()?.subscription?.plan?.id;
    return currentPlanId === plan.id;
  }
}
