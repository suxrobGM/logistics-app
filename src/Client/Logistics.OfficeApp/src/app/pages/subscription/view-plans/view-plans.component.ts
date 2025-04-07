import {CurrencyPipe} from "@angular/common";
import {Component, OnInit, signal} from "@angular/core";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ApiService} from "@/core/api";
import {SubscriptionPlanDto} from "@/core/api/models";
import {TenantService} from "@/core/services";

@Component({
  selector: "app-view-plans",
  imports: [CardModule, CurrencyPipe, ButtonModule],
  templateUrl: "./view-plans.component.html",
})
export class ViewPlansComponent implements OnInit {
  readonly subscriptionPlans = signal<SubscriptionPlanDto[]>([]);

  constructor(
    private readonly apiService: ApiService,
    private readonly tenantService: TenantService
  ) {}

  ngOnInit(): void {
    this.apiService.getSubscriptionPlans().subscribe((result) => {
      if (result.success && result.data) {
        this.subscriptionPlans.set(result.data);
      }
    });
  }

  isCurrentPlan(plan: SubscriptionPlanDto): boolean {
    const currentPlanId = this.tenantService.getTenantData()?.subscription?.plan?.id;
    return currentPlanId === plan.id;
  }
}
