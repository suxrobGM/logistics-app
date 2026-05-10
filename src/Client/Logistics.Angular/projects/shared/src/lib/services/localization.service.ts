import { inject, Injectable, InjectionToken } from "@angular/core";
import type { TenantSettings } from "../api/generated/models/tenant-settings";
import type { DistanceUnitTypes, WeightUnitTypes } from "../utils/converters";

/**
 * Interface for providing tenant settings to the localization service.
 * Applications should implement this and provide it via the TENANT_SETTINGS_PROVIDER token.
 */
export interface TenantSettingsProvider {
  getSettings(): TenantSettings;
}

/**
 * Injection token for the tenant settings provider.
 * Applications must provide an implementation of TenantSettingsProvider.
 */
export const TENANT_SETTINGS_PROVIDER = new InjectionToken<TenantSettingsProvider>(
  "TenantSettingsProvider",
);

/**
 * Service for accessing tenant-specific localization settings.
 * Provides methods to get distance units, currency, date formats, etc.
 */
@Injectable({ providedIn: "root" })
export class LocalizationService {
  private readonly settingsProvider = inject(TENANT_SETTINGS_PROVIDER);

  /**
   * Gets the distance unit code for conversions.
   * @returns 'mi' or 'km'
   */
  getDistanceUnit(): DistanceUnitTypes {
    const settings = this.settingsProvider.getSettings();
    const unit = settings.distanceUnit;
    return unit === "kilometers" ? "km" : "mi";
  }

  /**
   * Gets the distance unit label for display.
   * @returns 'mi' or 'km'
   */
  getDistanceUnitLabel(): string {
    return this.getDistanceUnit();
  }

  /**
   * Gets the full distance unit name for display in headers.
   * @returns 'Miles' or 'Kilometers'
   */
  getDistanceUnitName(): string {
    const settings = this.settingsProvider.getSettings();
    const unit = settings.distanceUnit;
    return unit === "kilometers" ? "Kilometers" : "Miles";
  }

  /**
   * Gets the currency code.
   * @returns Currency code (e.g., 'USD', 'EUR')
   */
  getCurrencyCode(): string {
    const settings = this.settingsProvider.getSettings();
    return (settings.currency ?? "usd").toUpperCase();
  }

  /**
   * Gets the currency symbol for display.
   * @returns Currency symbol (e.g., '$', '€', '£')
   */
  getCurrencySymbol(): string {
    const symbols: Record<string, string> = {
      USD: "$",
      EUR: "€",
      GBP: "£",
      CAD: "CA$",
      MXN: "MX$",
      AUD: "A$",
    };
    return symbols[this.getCurrencyCode()] ?? "$";
  }

  /**
   * Gets the date format type.
   * @returns 'us', 'european', or 'iso'
   */
  getDateFormatType(): string {
    const settings = this.settingsProvider.getSettings();
    return settings.dateFormat ?? "us";
  }

  /**
   * Gets the Angular date format string.
   * @returns Angular date format pattern
   */
  getDateFormat(): string {
    const formats: Record<string, string> = {
      us: "MM/dd/yyyy",
      european: "dd/MM/yyyy",
      iso: "yyyy-MM-dd",
    };
    return formats[this.getDateFormatType()] ?? "MM/dd/yyyy";
  }

  /**
   * Gets the Angular datetime format string.
   * @returns Angular datetime format pattern
   */
  getDateTimeFormat(): string {
    const formats: Record<string, string> = {
      us: "MM/dd/yyyy h:mm a",
      european: "dd/MM/yyyy HH:mm",
      iso: "yyyy-MM-dd HH:mm",
    };
    return formats[this.getDateFormatType()] ?? "MM/dd/yyyy h:mm a";
  }

  /**
   * Gets the timezone.
   * @returns IANA timezone string (e.g., 'America/New_York')
   */
  getTimezone(): string {
    const settings = this.settingsProvider.getSettings();
    return settings.timezone ?? "America/New_York";
  }

  /**
   * Gets the weight unit code for conversions.
   * @returns 'lbs' or 'kg'
   */
  getWeightUnit(): WeightUnitTypes {
    const settings = this.settingsProvider.getSettings();
    const unit = settings.weightUnit;
    return unit === "kilograms" ? "kg" : "lbs";
  }

  /**
   * Gets the weight unit label for display.
   * @returns 'lbs' or 'kg'
   */
  getWeightUnitLabel(): string {
    return this.getWeightUnit();
  }

  /**
   * Gets the full weight unit name for display.
   * @returns 'Pounds' or 'Kilograms'
   */
  getWeightUnitName(): string {
    const settings = this.settingsProvider.getSettings();
    const unit = settings.weightUnit;
    return unit === "kilograms" ? "Kilograms" : "Pounds";
  }

  /**
   * Formats a number as currency using tenant settings.
   * @param value The numeric value
   * @returns Formatted currency string
   */
  formatCurrency(value: number): string {
    const currencyCode = this.getCurrencyCode();
    const locale = this.getLocale() ?? "en-US";
    return new Intl.NumberFormat(locale, {
      style: "currency",
      currency: currencyCode,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value);
  }

  /**
   * Gets the tenant's operating region.
   */
  getRegion(): string {
    return this.settingsProvider.getSettings().region ?? "us";
  }

  /**
   * Gets the BCP-47 locale combining the tenant's language and region
   * (e.g., 'en-US', 'de-DE'). Used by Intl.* formatters.
   */
  getLocale(): string {
    const settings = this.settingsProvider.getSettings();
    const language = (settings.language ?? "en").toLowerCase();
    const country = this.getCountryForRegion(this.getRegion());
    return country ? `${language}-${country}` : language;
  }

  private getCountryForRegion(region: string): string | null {
    switch (region.toLowerCase()) {
      case "us":
        return "US";
      case "eu":
        return "DE";
      default:
        return null;
    }
  }

  /**
   * Returns the region-aware tax label used on invoices, PDFs, and forms.
   * @returns 'VAT' for EU tenants, 'Sales Tax' for US, 'Tax' otherwise
   */
  getTaxLabel(): string {
    switch (this.getRegion()) {
      case "eu":
        return "VAT";
      case "us":
        return "Sales Tax";
      default:
        return "Tax";
    }
  }

  /**
   * Formats a tax rate percentage with locale-aware decimal separator.
   * @param percent Rate as a number (e.g. 19, 21.5). Zero / null renders as a dash.
   */
  formatTaxRate(percent: number | null | undefined): string {
    if (percent == null || percent <= 0) {
      return "—";
    }
    const locale = this.getLocale() ?? "en-US";
    const formatted = new Intl.NumberFormat(locale, {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(percent);
    return `${formatted}%`;
  }
}
