import { Component, inject } from "@angular/core";
import { Icon, Stack, Surface, Typography, UiButton } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";

@Component({
  selector: "cp-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [Icon, Stack, Surface, Typography, UiButton],
})
export class Unauthorized {
  private readonly authService = inject(AuthService);

  protected login(): void {
    this.authService.login();
  }

  protected logout(): void {
    this.authService.logout();
  }
}
