import { CurrencyPipe } from "@angular/common";
import { Pipe, type PipeTransform, inject } from "@angular/core";
import { LocalizationService } from "../services/localization.service";

@Pipe({ name: "currencyFormat", standalone: true })
export class CurrencyFormatPipe implements PipeTransform {
  private readonly localizationService = inject(LocalizationService, { optional: true });
  private readonly currencyPipe = new CurrencyPipe("en-US");

  /**
   * Formats a numeric value as currency using tenant settings.
   * @param value The numeric value to format.
   * @param currencyCode Optional currency code override. Uses tenant settings if not provided.
   * @param display Currency display format: 'symbol', 'code', or 'symbol-narrow'. Defaults to 'symbol'.
   * @param digitsInfo Decimal representation. Defaults to '1.2-2'.
   * @returns Formatted currency string or null if value is null/undefined.
   */
  transform(
    value?: number | null,
    currencyCode?: string,
    display: "code" | "symbol" | "symbol-narrow" = "symbol",
    digitsInfo = "1.2-2",
  ): string | null {
    if (value == null) {
      return null;
    }

    const code = currencyCode ?? this.localizationService?.getCurrencyCode() ?? "USD";
    return this.currencyPipe.transform(value, code, display, digitsInfo);
  }
}
