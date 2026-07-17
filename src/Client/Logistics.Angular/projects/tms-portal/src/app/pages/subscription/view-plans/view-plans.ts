import { CurrencyPipe } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  changeSubscriptionPlan,
  createSubscription,
  getSubscriptionPlans,
  type SubscriptionPlanDto,
  type TenantFeature,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { TenantService, ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { Labels } from "@/shared/utils";

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
  agentic_dispatch: "AI Dispatch",
  priority_support: "Priority Support",
  api_access: "API Access",
  telegram_bot: "Telegram Bot",
  mcp_server: "MCP Server",
  accounting: "Accounting (QuickBooks)",
  fuel_cards: "Fuel Cards",
  ifta: "IFTA Reporting",
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
  "agentic_dispatch",
  "priority_support",
  "api_access",
  "telegram_bot",
  "mcp_server",
  "accounting",
  "fuel_cards",
  "ifta",
];

@Component({
  selector: "app-view-plans",
  templateUrl: "./view-plans.html",
  imports: [
    Badge,
    Card,
    CurrencyPipe,
    Grid,
    Icon,
    PageHeader,
    Stack,
    Surface,
    Typography,
    UiButton,
  ],
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

  protected getTierSeverity(plan: SubscriptionPlanDto): UiBadgeIntent {
    return plan.tier ? Labels.planTierSeverity(plan.tier) : "info";
  }

  protected getMaxTrucksLabel(plan: SubscriptionPlanDto): string {
    return plan.maxTrucks ? `Up to ${plan.maxTrucks} trucks` : "Unlimited trucks";
  }

  protected getAiDispatchLabel(plan: SubscriptionPlanDto): string {
    switch (plan.tier) {
      case "enterprise":
        return "unlimited usage";
      case "professional":
        return "higher usage";
      default:
        return "included";
    }
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
      icon: "payment",
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
