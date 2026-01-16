import { Component, inject } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { AuthService } from "@/core/auth";

@Component({
  selector: "cp-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [ButtonModule],
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
