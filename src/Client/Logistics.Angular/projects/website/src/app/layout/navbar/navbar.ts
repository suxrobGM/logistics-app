import { Component, output, signal } from "@angular/core";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.html",
  imports: [ButtonModule],
  host: {
    "(window:scroll)": "onScroll()",
  },
})
export class Navbar {
  protected readonly scrolled = signal(false);
  protected readonly mobileMenuOpen = signal(false);
  public readonly demoRequested = output<void>();

  protected onScroll(): void {
    this.scrolled.set(window.scrollY > 50);
  }
}
