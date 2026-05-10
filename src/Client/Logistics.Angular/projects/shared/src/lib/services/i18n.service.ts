import { inject, Injectable, signal } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { TENANT_SETTINGS_PROVIDER } from "./localization.service";

export interface I18nInitOptions {
  supportedLanguages: string[];
  defaultLanguage?: string;
}

/**
 * Wraps {@link TranslateService} so portals share a single language-switching API.
 * The picker, settings page, and any auto-detection code go through {@link setLanguage}.
 *
 * Foundation phase: read tenant + browser language at startup, expose `currentLanguage`
 * as a signal so components can react. Persisting the user choice to the backend will
 * land in a follow-up once `PUT /api/users/me/preferred-language` exists.
 */
@Injectable({ providedIn: "root" })
export class I18nService {
  private readonly translate = inject(TranslateService);
  private readonly settingsProvider = inject(TENANT_SETTINGS_PROVIDER);

  readonly currentLanguage = signal<string>("en");

  init(options: I18nInitOptions): void {
    const fallback = options.defaultLanguage ?? "en";
    this.translate.addLangs(options.supportedLanguages);
    this.translate.setFallbackLang(fallback);

    const initial = this.resolveInitialLanguage(options.supportedLanguages, fallback);
    this.translate.use(initial);
    this.currentLanguage.set(initial);
  }

  setLanguage(code: string): void {
    this.translate.use(code);
    this.currentLanguage.set(code);
    // Persistence to User.PreferredLanguage is deferred until the backend write endpoint exists.
  }

  private resolveInitialLanguage(supported: string[], fallback: string): string {
    const settings = this.settingsProvider.getSettings();
    const tenantLang = settings.language;
    if (tenantLang && supported.includes(tenantLang)) {
      return tenantLang;
    }

    const browser = typeof navigator !== "undefined" ? navigator.language?.split("-")[0] : null;
    if (browser && supported.includes(browser)) {
      return browser;
    }
    return fallback;
  }
}
