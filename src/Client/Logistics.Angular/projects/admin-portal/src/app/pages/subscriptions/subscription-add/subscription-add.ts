import { Component, inject, type OnInit, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterModule } from "@angular/router";
import { Api, createSubscription, getSubscriptionPlans, getTenants } from "@logistics/shared/api";
import type { CreateSubscriptionCommand, SubscriptionPlanDto, TenantDto } from "@logistics/shared/api/models";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SelectModule } from "primeng/select";
import { SkeletonModule } from "primeng/skeleton";

interface SelectOption {
  label: string;
  value: string;
}

@Component({
  selector: "adm-subscription-add",
  templateUrl: "./subscription-add.html",
  imports: [
    CardModule,
    ButtonModule,
    RouterModule,
    DividerModule,
    SkeletonModule,
    SelectModule,
    ReactiveFormsModule,
    LabeledField,
    ValidationSummary,
  ],
})
export class SubscriptionAdd implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isFetching = signal<boolean>(true);
  protected readonly tenantOptions = signal<SelectOption[]>([]);
  protected readonly planOptions = signal<SelectOption[]>([]);

  protected readonly form = new FormGroup({
    tenantId: new FormControl("", { validators: Validators.required, nonNullable: true }),
    planId: new FormControl("", { validators: Validators.required, nonNullable: true }),
  });

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
      (t) => !t.subscription || t.subscription.status === "cancelled" || t.subscription.status === "unpaid"
    );

    this.tenantOptions.set(
      tenantsWithoutSubscription.map((t) => ({
        label: `${t.name} (${t.companyName})`,
        value: t.id!,
      }))
    );

    this.planOptions.set(
      plans.map((p) => ({
        label: `${p.name} - $${p.price}/${p.interval}`,
        value: p.id!,
      }))
    );

    this.isFetching.set(false);
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isLoading.set(true);

    const command: CreateSubscriptionCommand = {
      tenantId: this.form.value.tenantId!,
      planId: this.form.value.planId!,
    };

    await this.api.invoke(createSubscription, { body: command });
    this.toastService.showSuccess("Subscription has been created successfully");
    this.router.navigateByUrl("/subscriptions");

    this.isLoading.set(false);
  }
}
