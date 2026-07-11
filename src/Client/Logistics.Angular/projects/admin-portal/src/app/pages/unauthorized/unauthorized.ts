import { Component, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Icon, UiButton } from "@logistics/shared/ui";
import { CardModule } from "primeng/card";
import { AuthService } from "@/core/auth";

@Component({
  selector: "adm-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [CardModule, Icon, RouterModule, UiButton],
})
export class Unauthorized {
  private readonly authService = inject(AuthService);

  protected logout(): void {
    this.authService.logout();
  }
}
