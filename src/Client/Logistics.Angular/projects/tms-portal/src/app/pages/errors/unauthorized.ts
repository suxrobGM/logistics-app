import { Location } from "@angular/common";
import { Component, computed, effect, inject, input } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { AuthService } from "@/core/auth";
import { TenantService } from "@/core/services";

@Component({
  selector: "app-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [RouterLink, ButtonModule],
})
export class UnauthorizedComponent {
  private readonly location = inject(Location);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);

  protected readonly reason = input<string | null>();

  protected readonly isSubscriptionIssue = computed(() => this.reason() === "subscription");

  constructor() {
    effect(() => {
      this.tenantService.tenantData();
      if (this.reason() === "subscription" && this.tenantService.isSubscriptionActive()) {
        console.log("Subscription is active, redirecting to home page");
        this.router.navigate(["/"]);
      }
    });
  }

  protected goBack(): void {
    this.location.back();
  }

  protected logout(): void {
    this.authService.logout();
  }
}
