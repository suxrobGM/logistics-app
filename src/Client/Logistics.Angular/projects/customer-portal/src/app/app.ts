import { Component, inject } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { ConfirmDialog } from "primeng/confirmdialog";
import { ToastModule } from "primeng/toast";
import { AuthService } from "@/core/auth";

@Component({
  selector: "cp-root",
  templateUrl: "./app.html",
  imports: [RouterOutlet, ToastModule, ConfirmDialog],
})
export class App {
  private readonly authService = inject(AuthService);

  constructor() {
    // Initialize auth check on app start
    this.authService.checkAuth().subscribe();
  }
}
