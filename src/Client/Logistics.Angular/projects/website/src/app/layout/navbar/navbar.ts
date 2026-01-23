import { Component, computed, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
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
  private readonly router = inject(Router);
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly scrolled = signal(false);
  protected readonly mobileMenuOpen = signal(false);

  // Only home page has dark hero - allow transparent navbar there
  protected readonly hasDarkHero = computed(() => {
    const url = this.router.url.split("?")[0]; // Remove query params
    return url === "/";
  });

  // Show solid navbar when scrolled OR on pages without dark hero
  protected readonly showSolidNavbar = computed(() => this.scrolled() || !this.hasDarkHero());

  protected onScroll(): void {
    this.scrolled.set(window.scrollY > 50);
  }

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
