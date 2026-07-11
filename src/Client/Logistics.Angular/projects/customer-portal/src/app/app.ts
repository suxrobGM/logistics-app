import { Component, inject } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { CookieBanner, UiToaster } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";

@Component({
  selector: "cp-root",
  templateUrl: "./app.html",
  imports: [UiToaster, RouterOutlet, CookieBanner],
})
export class App {
  private readonly authService = inject(AuthService);

  constructor() {
    // Initialize auth check on app start
    this.authService.checkAuth().subscribe();
  }
}
