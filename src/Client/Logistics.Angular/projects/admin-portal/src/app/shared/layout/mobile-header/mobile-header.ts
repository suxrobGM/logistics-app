import { Component, inject } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { LayoutService } from "@/core/services/layout.service";

@Component({
  selector: "adm-mobile-header",
  templateUrl: "./mobile-header.html",
  imports: [ButtonModule],
})
export class MobileHeader {
  private readonly layoutService = inject(LayoutService);

  protected readonly mobileMenuOpen = this.layoutService.mobileMenuOpen;

  protected toggleMenu(): void {
    this.layoutService.toggleMobileMenu();
  }
}
