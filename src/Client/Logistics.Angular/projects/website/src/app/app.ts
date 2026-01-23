import { Component, inject } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { ConfirmDialog } from "primeng/confirmdialog";
import { ToastModule } from "primeng/toast";
import { Footer, Navbar } from "@/layout";
import { DemoDialog } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";

@Component({
  selector: "web-root",
  templateUrl: "./app.html",
  imports: [RouterOutlet, Navbar, Footer, DemoDialog, ToastModule, ConfirmDialog],
})
export class App {
  protected readonly demoDialogService = inject(DemoDialogService);
}
