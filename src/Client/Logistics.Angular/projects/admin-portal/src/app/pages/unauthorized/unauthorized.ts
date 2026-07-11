import { Component, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Card, Icon, UiButton } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";

@Component({
  selector: "adm-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [Card, Icon, RouterModule, UiButton],
})
export class Unauthorized {
  private readonly authService = inject(AuthService);

  protected logout(): void {
    this.authService.logout();
  }
}
