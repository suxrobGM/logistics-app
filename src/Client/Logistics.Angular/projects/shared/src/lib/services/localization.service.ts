import { inject, Injectable, InjectionToken } from "@angular/core";
import type { TenantSettings } from "../api/generated/models/tenant-settings";
import {
  Converters,
  type DistanceUnitTypes,
  type FuelEfficiencyUnitTypes,
  type TemperatureUnitTypes,
  type VolumeUnitTypes,
  type WeightUnitTypes,
} from "../utils/converters";

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
   * Gets the PrimeNG p-datepicker date format string. PrimeNG uses a different
   * syntax to the Angular date pipe (lowercase, single chars).
   * @returns PrimeNG-compatible pattern
   */
  getPrimeNgDateFormat(): string {
    const formats: Record<string, string> = {
      us: "mm/dd/yy",
      european: "dd/mm/yy",
      iso: "yy-mm-dd",
    };
    return formats[this.getDateFormatType()] ?? "mm/dd/yy";
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
   * Gets the volume unit code for conversions.
   * @returns 'gal' or 'L'
   */
  getVolumeUnit(): VolumeUnitTypes {
    const settings = this.settingsProvider.getSettings();
    return settings.volumeUnit === "liters" ? "L" : "gal";
  }

  /**
   * Gets the volume unit label for display.
   * @returns 'gal' or 'L'
   */
  getVolumeUnitLabel(): string {
    return this.getVolumeUnit();
  }

  /**
   * Gets the full volume unit name for display.
   * @returns 'Gallons' or 'Liters'
   */
  getVolumeUnitName(): string {
    return this.getVolumeUnit() === "L" ? "Liters" : "Gallons";
  }

  /**
   * Formats a volume value, converting to the tenant unit when a source unit is provided.
   * @param value Volume value
   * @param fromUnit Source unit; defaults to tenant unit (no conversion)
   * @returns Formatted string like "10.5 gal"
   */
  formatVolume(value: number, fromUnit?: VolumeUnitTypes): string {
    const target = this.getVolumeUnit();
    const converted = fromUnit ? Converters.convertVolume(value, fromUnit, target) : value;
    return `${converted} ${target}`;
  }

  /**
   * Gets the temperature unit code for conversions.
   * @returns 'F' or 'C'
   */
  getTemperatureUnit(): TemperatureUnitTypes {
    const settings = this.settingsProvider.getSettings();
    return settings.temperatureUnit === "celsius" ? "C" : "F";
  }

  /**
   * Gets the temperature unit label for display.
   * @returns '°F' or '°C'
   */
  getTemperatureUnitLabel(): string {
    return `°${this.getTemperatureUnit()}`;
  }

  /**
   * Gets the full temperature unit name for display.
   * @returns 'Fahrenheit' or 'Celsius'
   */
  getTemperatureUnitName(): string {
    return this.getTemperatureUnit() === "C" ? "Celsius" : "Fahrenheit";
  }

  /**
   * Formats a temperature value, converting to the tenant unit when a source unit is provided.
   * @param value Temperature value
   * @param fromUnit Source unit; defaults to tenant unit (no conversion)
   * @returns Formatted string like "14°F"
   */
  formatTemperature(value: number, fromUnit?: TemperatureUnitTypes): string {
    const target = this.getTemperatureUnit();
    const converted = fromUnit ? Converters.convertTemperature(value, fromUnit, target) : value;
    return `${converted}°${target}`;
  }

  /**
   * Gets the fuel-efficiency unit derived from the tenant's volume unit.
   * @returns 'mpg' for gallons, 'L/100km' for liters
   */
  getFuelEfficiencyUnit(): FuelEfficiencyUnitTypes {
    return this.getVolumeUnit() === "L" ? "L/100km" : "mpg";
  }

  /**
   * Formats a fuel-efficiency value, converting to the tenant unit when a source unit is provided.
   * @param value Efficiency value
   * @param fromUnit Source unit; defaults to tenant unit (no conversion)
   * @returns Formatted string like "8.5 L/100km"
   */
  formatFuelEfficiency(value: number, fromUnit?: FuelEfficiencyUnitTypes): string {
    const target = this.getFuelEfficiencyUnit();
    const converted = fromUnit ? Converters.convertFuelEfficiency(value, fromUnit, target) : value;
    return `${converted} ${target}`;
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
   * Formats an HOS duration in minutes using locale-appropriate spacing.
   * US: "11h 30m" (compact). EU: "11 h 30 min" (with spaces, per regulation labelling).
   * Negative values are rendered as zero; nulls return a dash.
   */
  formatHosDuration(minutes: number | null | undefined): string {
    if (minutes == null) {
      return "—";
    }
    const total = Math.max(0, Math.trunc(minutes));
    const h = Math.trunc(total / 60);
    const m = total % 60;
    return this.getRegion() === "eu" ? `${h} h ${m} min` : `${h}h ${m}m`;
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
