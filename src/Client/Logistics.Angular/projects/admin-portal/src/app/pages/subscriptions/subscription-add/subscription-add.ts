import { Component, inject, signal, type OnInit } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import {
  Api,
  createSubscription,
  getSubscriptionPlans,
  getTenants,
  type CreateSubscriptionCommand,
  type SubscriptionPlanDto,
  type TenantDto,
} from "@logistics/shared/api";
import {
  Card,
  Grid,
  Icon,
  PageHeader,
  Skeleton,
  Stack,
  Typography,
  UiButton,
  UiFormField,
  UiSelectField,
  ValidatedForm,
} from "@logistics/shared/ui";

interface SelectOption {
  label: string;
  value: string;
}

const EMPTY = { tenantId: "", planId: "" };

@Component({
  selector: "adm-subscription-add",
  templateUrl: "./subscription-add.html",
  imports: [
    Card,
    FormField,
    FormRoot,
    Grid,
    Icon,
    PageHeader,
    RouterModule,
    Skeleton,
    Stack,
    Typography,
    UiButton,
    UiFormField,
    UiSelectField,
    ValidatedForm,
  ],
})
export class SubscriptionAdd implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isFetching = signal<boolean>(true);
  protected readonly tenantOptions = signal<SelectOption[]>([]);
  protected readonly planOptions = signal<SelectOption[]>([]);

  protected readonly model = signal({ ...EMPTY });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.tenantId, { message: "Tenant is required." });
      required(p.planId, { message: "Subscription plan is required." });
    },
    {
      submission: {
        action: async () => {
          const command: CreateSubscriptionCommand = {
            tenantId: this.model().tenantId,
            planId: this.model().planId,
          };

          await this.api.invoke(createSubscription, { body: command });
          this.toastService.showSuccess("Subscription has been created successfully");
          this.router.navigateByUrl("/subscriptions");
          return undefined;
        },
      },
    },
  );

  ngOnInit(): void {
    this.fetchOptions();
  }

  private async fetchOptions(): Promise<void> {
    this.isFetching.set(true);

    const [tenantsResult, plansResult] = await Promise.all([
      this.api.invoke(getTenants, { PageSize: 1000 }),
      this.api.invoke(getSubscriptionPlans, { PageSize: 1000 }),
    ]);

    const tenants = (tenantsResult?.items ?? []) as TenantDto[];
    const plans = (plansResult?.items ?? []) as SubscriptionPlanDto[];

    // Filter out tenants that already have an active subscription
    const tenantsWithoutSubscription = tenants.filter(
      (t) =>
        !t.subscription ||
        t.subscription.status === "cancelled" ||
        t.subscription.status === "unpaid",
    );

    this.tenantOptions.set(
      tenantsWithoutSubscription.map((t) => ({
        label: `${t.name} (${t.companyName})`,
        value: t.id!,
      })),
    );

    this.planOptions.set(
      plans.map((p) => ({
        label: `${p.name} - $${p.price}/mo + $${p.perTruckPrice}/truck`,
        value: p.id!,
      })),
    );

    this.isFetching.set(false);
  }
}
