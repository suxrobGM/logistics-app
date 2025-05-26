import {Component, signal} from "@angular/core";
import {Router} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {AuthService} from "@/core/auth";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.css",
  imports: [ProgressSpinnerModule, ButtonModule],
})
export class LoginComponent {
  public readonly isAuthenticated = signal(false);
  public readonly isLoading = signal(false);

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
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

  login() {
    this.isLoading.set(true);
    this.authService.login();
  }

  private redirectToHome() {
    if (this.isAuthenticated()) {
      this.router.navigateByUrl("/home");
    }
  }
}
