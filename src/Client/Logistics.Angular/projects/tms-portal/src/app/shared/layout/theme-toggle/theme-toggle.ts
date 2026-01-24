import { Component, inject, input } from "@angular/core";
import { TooltipModule } from "primeng/tooltip";
import { ThemeService } from "@/core/services";

@Component({
  selector: "app-theme-toggle",
  templateUrl: "./theme-toggle.html",
  styleUrl: "./theme-toggle.css",
  imports: [TooltipModule],
})
export class ThemeToggle {
  private readonly themeService = inject(ThemeService);

  public readonly isDark = this.themeService.isDark;
  public readonly showLabel = input(true);

  public toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
