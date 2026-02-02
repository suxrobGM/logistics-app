import { DatePipe } from "@angular/common";
import { Pipe, type PipeTransform, inject } from "@angular/core";
import { LocalizationService } from "../services/localization.service";

@Pipe({ name: "dateFormat" })
export class DateFormatPipe implements PipeTransform {
  private readonly localizationService = inject(LocalizationService, { optional: true });
  private readonly datePipe = new DatePipe("en-US");

  /**
   * Formats a date value using tenant settings.
   * @param value The date value to format.
   * @param format Optional format string override. Uses tenant date format if not provided.
   *               Special values: 'short', 'medium', 'long', 'full', 'shortDate', 'mediumDate',
   *               'longDate', 'fullDate', 'shortTime', 'mediumTime', 'longTime', 'fullTime'.
   * @param timezone Optional timezone override. Uses tenant timezone if not provided.
   * @returns Formatted date string or null if value is null/undefined.
   */
  transform(
    value?: Date | string | number | null,
    format?: string,
    timezone?: string,
  ): string | null {
    if (!value) {
      return null;
    }

    // If a specific named format is provided, use it directly
    const namedFormats = [
      "short",
      "medium",
      "long",
      "full",
      "shortDate",
      "mediumDate",
      "longDate",
      "fullDate",
      "shortTime",
      "mediumTime",
      "longTime",
      "fullTime",
    ];

    let formatStr: string;
    if (format && namedFormats.includes(format)) {
      formatStr = format;
    } else {
      formatStr = format ?? this.localizationService?.getDateFormat() ?? "MM/dd/yyyy";
    }

    const tz = timezone ?? this.localizationService?.getTimezone();
    return this.datePipe.transform(value, formatStr, tz);
  }
}
