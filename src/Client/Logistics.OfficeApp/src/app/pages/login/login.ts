import {Component, inject, signal} from "@angular/core";
import {Router} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {AuthService} from "@/core/auth";

@Component({
  selector: "app-login",
  templateUrl: "./login.html",
  styleUrl: "./login.css",
  imports: [ProgressSpinnerModule, ButtonModule],
})
export class LoginComponent {
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
      this.router.navigateByUrl("/home");
    }
  }
}
