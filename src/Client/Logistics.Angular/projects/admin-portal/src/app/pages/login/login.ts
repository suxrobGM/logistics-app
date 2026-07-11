import { Component, inject, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { Icon, UiButton } from "@logistics/shared/ui";
import { CardModule } from "primeng/card";
import { AuthService } from "@/core/auth";

@Component({
  selector: "adm-login",
  templateUrl: "./login.html",
  imports: [CardModule, Icon, UiButton],
})
export class Login implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    // If already authenticated, redirect to home
    this.authService.onAuthenticated().subscribe((isAuthenticated) => {
      if (isAuthenticated) {
        this.router.navigate(["/home"]);
      }
    });
  }

  protected login(): void {
    this.authService.login();
  }
}
