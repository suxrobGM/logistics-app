import { CurrencyPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  changeSubscriptionPlan,
  createSubscription,
  getSubscriptionPlans,
} from "@logistics/shared/api";
import type { SubscriptionPlanDto, TenantFeature } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";
import { TenantService, ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { Labels, type SeverityLevel } from "@/shared/utils";

/** Display name mapping for tenant features */
const featureLabels: Record<TenantFeature, string> = {
  dashboard: "Dashboard",
  employees: "Employees",
  loads: "Loads",
  trucks: "Trucks",
  customers: "Customers",
  invoices: "Invoices",
  payments: "Payments",
  eld: "ELD / HOS",
  load_board: "Load Board",
  messages: "Messages",
  notifications: "Notifications",
  safety: "Safety & Compliance",
  expenses: "Expenses",
  payroll: "Payroll",
  timesheets: "Timesheets",
  maintenance: "Maintenance",
  trips: "Trips",
  reports: "Reports",
};

/** All features in display order */
const allFeatures: TenantFeature[] = [
  "dashboard",
  "employees",
  "loads",
  "trucks",
  "customers",
  "invoices",
  "payments",
  "trips",
  "messages",
  "notifications",
  "expenses",
  "reports",
  "eld",
  "load_board",
  "payroll",
  "timesheets",
  "safety",
  "maintenance",
];

@Component({
  selector: "app-view-plans",
  templateUrl: "./view-plans.html",
  imports: [CardModule, CurrencyPipe, ButtonModule, TagModule, PageHeader],
})
export class ViewPlansComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly subscriptionPlans = signal<SubscriptionPlanDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly allFeatures = allFeatures;
  protected readonly Labels = Labels;

  async ngOnInit(): Promise<void> {
    const result = await this.api.invoke(getSubscriptionPlans, {});
    if (result.items) {
      // Sort by tier order: starter, professional, enterprise
      const tierOrder = { starter: 0, professional: 1, enterprise: 2 };
      const sorted = [...result.items].sort(
        (a, b) => (tierOrder[a.tier!] ?? 0) - (tierOrder[b.tier!] ?? 0),
      );
      this.subscriptionPlans.set(sorted);
    }
  }

  protected isCurrentPlan(plan: SubscriptionPlanDto): boolean {
    const currentPlanId = this.tenantService.getTenantData()?.subscription?.plan?.id;
    return currentPlanId === plan.id;
  }

  protected getFeatureLabel(feature: TenantFeature): string {
    return featureLabels[feature] ?? feature;
  }

  protected planHasFeature(plan: SubscriptionPlanDto, feature: TenantFeature): boolean {
    return plan.features?.includes(feature) ?? false;
  }

  protected getTierSeverity(plan: SubscriptionPlanDto): SeverityLevel {
    return plan.tier ? Labels.planTierSeverity(plan.tier) : "info";
  }

  protected getMaxTrucksLabel(plan: SubscriptionPlanDto): string {
    return plan.maxTrucks ? `Up to ${plan.maxTrucks} trucks` : "Unlimited trucks";
  }

  protected selectPlan(plan: SubscriptionPlanDto): void {
    const subscription = this.tenantService.getTenantData()?.subscription;
    const hasSubscription = subscription != null;

    const message = hasSubscription
      ? `Are you sure you want to switch to the ${plan.name} plan? Your billing will be prorated.`
      : `Are you sure you want to subscribe to the ${plan.name} plan? You'll start with a free trial.`;

    this.toastService.confirm({
      message,
      header: hasSubscription ? "Change Plan" : "Subscribe to Plan",
      icon: "pi pi-credit-card",
      acceptLabel: hasSubscription ? "Yes, Switch" : "Yes, Subscribe",
      rejectLabel: "Cancel",
      accept: async () => {
        this.isLoading.set(true);

        if (hasSubscription) {
          await this.api.invoke(changeSubscriptionPlan, {
            id: subscription!.id as string,
            body: { newPlanId: plan.id ?? undefined },
          });
        } else {
          const tenantId = this.tenantService.getTenantId() ?? undefined;
          await this.api.invoke(createSubscription, {
            body: { tenantId, planId: plan.id ?? undefined },
          });
        }

        this.toastService.showSuccess(
          hasSubscription ? "Plan changed successfully" : "Subscription created successfully",
        );
        this.tenantService.refetchTenantData();
        this.router.navigateByUrl("/subscription/manage");

        this.isLoading.set(false);
      },
    });
  }
}
