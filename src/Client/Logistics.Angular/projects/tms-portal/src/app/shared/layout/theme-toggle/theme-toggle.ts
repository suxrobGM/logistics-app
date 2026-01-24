import { isPlatformBrowser } from "@angular/common";
import { Component, PLATFORM_ID, inject, input, signal } from "@angular/core";
import { TooltipModule } from "primeng/tooltip";

const THEME_STORAGE_KEY = "tms-theme";

@Component({
  selector: "app-theme-toggle",
  templateUrl: "./theme-toggle.html",
  styleUrl: "./theme-toggle.css",
  imports: [TooltipModule],
})
export class ThemeToggle {
  private readonly platformId = inject(PLATFORM_ID);

  public readonly isDark = signal(false);
  public readonly showLabel = input(true);

  constructor() {
    // Initialize theme from stored preference (default: light)
    if (isPlatformBrowser(this.platformId)) {
      const stored = localStorage.getItem(THEME_STORAGE_KEY);
      const isDarkMode = stored === "dark";
      this.isDark.set(isDarkMode);
      this.applyTheme(isDarkMode);
    }
  }

  public toggleTheme(): void {
    const newValue = !this.isDark();
    this.isDark.set(newValue);
    this.applyTheme(newValue);

    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(THEME_STORAGE_KEY, newValue ? "dark" : "light");
    }
  }

  private applyTheme(isDark: boolean): void {
    if (isPlatformBrowser(this.platformId)) {
      const html = document.documentElement;

      // Toggle theme classes - PrimeNG uses .dark-theme selector
      if (isDark) {
        html.classList.add("dark-theme");
        html.classList.remove("light-theme");
      } else {
        html.classList.remove("dark-theme");
        html.classList.add("light-theme");
      }
    }
  }
}
