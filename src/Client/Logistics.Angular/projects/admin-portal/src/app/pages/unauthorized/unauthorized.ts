import { Component, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Icon } from "@logistics/shared/ui";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { AuthService } from "@/core/auth";

@Component({
  selector: "adm-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [ButtonModule, CardModule, Icon, RouterModule],
})
export class Unauthorized {
  private readonly authService = inject(AuthService);

  protected logout(): void {
    this.authService.logout();
  }
}
