import { Component, inject } from "@angular/core";
import { ThemeService } from "../../../services/theme.service";
import { Icon } from "../../icons/icon/icon";

/**
 * A light/dark theme toggle wired to the shared {@link ThemeService}.
 * Any portal can drop `<ui-theme-toggle />` into its chrome to switch `.dark-theme`.
 */
@Component({
  selector: "ui-theme-toggle",
  templateUrl: "./theme-toggle.html",
  imports: [Icon],
})
export class ThemeToggle {
  private readonly themeService = inject(ThemeService);

  protected readonly isDark = this.themeService.isDark;

  protected toggle(): void {
    this.themeService.toggleTheme();
  }
}
