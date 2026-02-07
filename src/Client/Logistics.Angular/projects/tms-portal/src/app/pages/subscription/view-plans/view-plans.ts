import { CurrencyPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { Api, getSubscriptionPlans } from "@logistics/shared/api";
import type { SubscriptionPlanDto, TenantFeature } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TagModule } from "primeng/tag";
import { TenantService } from "@/core/services";
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

  protected readonly subscriptionPlans = signal<SubscriptionPlanDto[]>([]);
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
}
