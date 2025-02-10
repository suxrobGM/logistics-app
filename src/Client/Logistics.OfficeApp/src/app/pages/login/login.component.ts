import {Component, OnInit, signal} from "@angular/core";
import {Router} from "@angular/router";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ButtonModule} from "primeng/button";
import {AuthService} from "@/core/auth";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrl: "./login.component.scss",
  standalone: true,
  imports: [ProgressSpinnerModule, ButtonModule],
})
export class LoginComponent implements OnInit {
  public readonly isAuthenticated = signal(false);
  public readonly isLoading = signal(false);

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
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
