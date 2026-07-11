import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Spinner, Stack, Surface, Typography, UiButton } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";

@Component({
  selector: "cp-login",
  templateUrl: "./login.html",
  imports: [Spinner, Stack, Surface, Typography, UiButton],
})
export class Login {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isAuthenticated = signal(false);
  protected readonly isLoading = signal(false);

  constructor() {
    this.authService.onCheckingAuth().subscribe(() => this.isLoading.set(true));
    this.authService.onCheckingAuthFinished().subscribe(() => {
      this.isLoading.set(false);
      this.redirectToHome();
    });

    this.authService.onAuthenticated().subscribe((isAuthenticated) => {
      this.isAuthenticated.set(isAuthenticated);
      this.redirectToHome();
    });
  }

  login(): void {
    this.isLoading.set(true);
    this.authService.login();
  }

  private redirectToHome(): void {
    if (this.isAuthenticated()) {
      this.router.navigateByUrl("/");
    }
  }
}
