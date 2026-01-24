import { isPlatformBrowser } from "@angular/common";
import { Injectable, PLATFORM_ID, inject, signal } from "@angular/core";

const THEME_STORAGE_KEY = "tms-theme";

@Injectable({ providedIn: "root" })
export class ThemeService {
  private readonly platformId = inject(PLATFORM_ID);

  /** Whether dark mode is currently active */
  public readonly isDark = signal(false);

  constructor() {
    this.initializeTheme();
  }

  /** Toggle between light and dark themes */
  public toggleTheme(): void {
    const newValue = !this.isDark();
    this.isDark.set(newValue);
    this.applyTheme(newValue);
    this.persistTheme(newValue);
  }

  /** Set the theme explicitly */
  public setTheme(isDark: boolean): void {
    this.isDark.set(isDark);
    this.applyTheme(isDark);
    this.persistTheme(isDark);
  }

  private initializeTheme(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    const isDarkMode = stored ? stored === "dark" : prefersDark;
    this.isDark.set(isDarkMode);
    this.applyTheme(isDarkMode);
  }

  private applyTheme(isDark: boolean): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const html = document.documentElement;

    if (isDark) {
      html.classList.add("dark-theme");
      html.classList.remove("light-theme");
    } else {
      html.classList.remove("dark-theme");
      html.classList.add("light-theme");
    }
  }

  private persistTheme(isDark: boolean): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(THEME_STORAGE_KEY, isDark ? "dark" : "light");
    }
  }
}
