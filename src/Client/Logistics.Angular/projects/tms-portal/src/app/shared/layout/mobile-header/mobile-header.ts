import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { LayoutService } from "@/core/services/layout.service";

@Component({
  selector: "app-mobile-header",
  templateUrl: "./mobile-header.html",
  imports: [ButtonModule, RouterLink],
})
export class MobileHeader {
  private readonly layoutService = inject(LayoutService);
  protected readonly mobileMenuOpen = this.layoutService.mobileMenuOpen;

  protected toggleMenu(): void {
    this.layoutService.toggleMobileMenu();
  }
}
