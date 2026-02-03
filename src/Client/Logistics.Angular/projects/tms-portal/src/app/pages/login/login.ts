import { Component, type OnInit, inject, signal } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { AuthService } from "@/core/auth";

@Component({
  selector: "app-login",
  templateUrl: "./login.html",
  styleUrl: "./login.css",
  imports: [ProgressSpinnerModule, ButtonModule],
})
export class LoginComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

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

  ngOnInit(): void {
    // Auto-login when redirected from impersonation
    const autoLogin = this.route.snapshot.queryParamMap.get("autologin");
    if (autoLogin === "true") {
      this.login();
    }
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
