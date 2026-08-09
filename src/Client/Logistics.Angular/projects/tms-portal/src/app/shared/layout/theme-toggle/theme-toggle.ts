import { Component, inject } from "@angular/core";
import { Icon, UiTooltip } from "@logistics/shared/ui";
import { ThemeService } from "@/core/services";

@Component({
  selector: "app-theme-toggle",
  templateUrl: "./theme-toggle.html",
  styleUrl: "./theme-toggle.css",
  imports: [Icon, UiTooltip],
})
export class ThemeToggle {
  private readonly themeService = inject(ThemeService);

  public readonly isDark = this.themeService.isDark;

  public toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
