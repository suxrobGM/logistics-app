import { Component, inject } from "@angular/core";
import { UiButton } from "@logistics/shared/ui";
import { LayoutService } from "@/core/services/layout.service";

@Component({
  selector: "adm-mobile-header",
  templateUrl: "./mobile-header.html",
  imports: [UiButton],
})
export class MobileHeader {
  private readonly layoutService = inject(LayoutService);

  protected readonly mobileMenuOpen = this.layoutService.mobileMenuOpen;

  protected toggleMenu(): void {
    this.layoutService.toggleMobileMenu();
  }
}
