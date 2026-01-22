import { Component, inject } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { Footer, Navbar } from "@/layout";
import { DemoDialog } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";

@Component({
  selector: "web-root",
  templateUrl: "./app.html",
  imports: [RouterOutlet, Navbar, Footer, DemoDialog],
})
export class App {
  protected readonly demoDialogService = inject(DemoDialogService);
}
