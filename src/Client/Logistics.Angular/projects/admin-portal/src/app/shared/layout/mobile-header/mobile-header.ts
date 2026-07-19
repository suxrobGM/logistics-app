import { Component, inject } from "@angular/core";
import { LayoutService } from "@logistics/shared/services";
import { UiButton } from "@logistics/shared/ui";

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
