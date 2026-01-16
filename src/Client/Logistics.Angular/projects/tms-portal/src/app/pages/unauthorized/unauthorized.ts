import { Component, inject, input } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { TenantService } from "@/core/services";

@Component({
  selector: "app-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [RouterModule],
})
export class UnauthorizedComponent {
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);

  protected readonly reason = input<string | null>();

  constructor() {
    this.tenantService.tenantDataChanged$.subscribe(() => {
      if (this.reason() === "subscription" && this.tenantService.isSubscriptionActive()) {
        console.log("Subscription is active, redirecting to home page");
        this.router.navigate(["/"]);
      }
    });
  }

  protected getReasonMessage(): string {
    if (this.reason() === "subscription") {
      return "Your subscription is not active. Please renew it.";
    }

    return "You do not have permission to access this page.";
  }
}
