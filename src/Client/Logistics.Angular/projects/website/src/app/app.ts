import { Component, inject } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { CookieBanner, UiToaster } from "@logistics/shared/ui";
import { Footer, Navbar } from "@/layout";
import { DemoDialog } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";

@Component({
  selector: "web-root",
  templateUrl: "./app.html",
  imports: [UiToaster, RouterOutlet, Navbar, Footer, DemoDialog, CookieBanner],
})
export class App {
  protected readonly demoDialogService = inject(DemoDialogService);
}
