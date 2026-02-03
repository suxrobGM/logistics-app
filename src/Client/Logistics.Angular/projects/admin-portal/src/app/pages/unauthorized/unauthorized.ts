import { Component, inject } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { AuthService } from "@/core/auth";

@Component({
  selector: "adm-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [ButtonModule, CardModule, RouterModule],
})
export class Unauthorized {
  private readonly authService = inject(AuthService);

  protected logout(): void {
    this.authService.logout();
  }
}
