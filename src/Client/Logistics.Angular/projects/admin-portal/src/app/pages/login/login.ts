import { Component, inject, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { AuthService } from "@/core/auth";

@Component({
  selector: "adm-login",
  templateUrl: "./login.html",
  imports: [ButtonModule, CardModule],
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
