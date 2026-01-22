import { Component, inject, signal } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { DemoDialogService } from "@/shared/services";

@Component({
  selector: "web-navbar",
  templateUrl: "./navbar.html",
  imports: [ButtonModule],
  host: {
    "(window:scroll)": "onScroll()",
  },
})
export class Navbar {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly scrolled = signal(false);
  protected readonly mobileMenuOpen = signal(false);

  protected onScroll(): void {
    this.scrolled.set(window.scrollY > 50);
  }

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
