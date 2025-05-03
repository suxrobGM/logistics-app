import {Component, input} from "@angular/core";
import {Router, RouterModule} from "@angular/router";
import {TenantService} from "@/core/services";

@Component({
  selector: "app-unauthorized",
  templateUrl: "./unauthorized.component.html",
  imports: [RouterModule],
})
export class UnauthorizedComponent {
  readonly reason = input<string | null>();

  constructor(
    readonly tenantService: TenantService,
    readonly router: Router
  ) {
    this.tenantService.tenantDataChanged$.subscribe(() => {
      if (this.reason() === "subscription" && this.tenantService.isSubscriptionActive()) {
        console.log("Subscription is active, redirecting to home page");
        this.router.navigate(["/"]);
      }
    });
  }

  getReasonMessage(): string {
    if (this.reason() === "subscription") {
      return "Your subscription is not active. Please renew it.";
    }

    return "You do not have permission to access this page.";
  }
}
