import { Component, inject, type OnInit, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, getTenants, getSubscriptions, getSubscriptionPlans, getUsers } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";

@Component({
  selector: "adm-home",
  templateUrl: "./home.html",
  imports: [CardModule, ButtonModule, RouterLink, DividerModule, SkeletonModule],
})
export class Home implements OnInit {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(true);
  protected readonly tenantCount = signal(0);
  protected readonly subscriptionCount = signal(0);
  protected readonly planCount = signal(0);
  protected readonly userCount = signal(0);

  ngOnInit(): void {
    this.loadStats();
  }

  private async loadStats(): Promise<void> {
    this.isLoading.set(true);

    const [tenants, subscriptions, plans, users] = await Promise.all([
      this.api.invoke(getTenants, { PageSize: 1 }),
      this.api.invoke(getSubscriptions, { PageSize: 1 }),
      this.api.invoke(getSubscriptionPlans, { PageSize: 1 }),
      this.api.invoke(getUsers, { PageSize: 1 }),
    ]);

    this.tenantCount.set(tenants?.pagination?.total ?? 0);
    this.subscriptionCount.set(subscriptions?.pagination?.total ?? 0);
    this.planCount.set(plans?.pagination?.total ?? 0);
    this.userCount.set(users?.pagination?.total ?? 0);
    this.isLoading.set(false);
  }
}
