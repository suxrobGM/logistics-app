import {Component, input} from "@angular/core";
import { RouterModule } from "@angular/router";

@Component({
  selector: "app-unauthorized",
  templateUrl: "./unauthorized.component.html",
  imports: [
    RouterModule
  ]
})
export class UnauthorizedComponent {
  readonly reason = input<string | null>()

  getReasonMessage(): string {
    if (this.reason() === "subscription") {
      return "Your subscription is not active. Please renew it.";
    }

    return "You do not have permission to access this page.";
  }
}
