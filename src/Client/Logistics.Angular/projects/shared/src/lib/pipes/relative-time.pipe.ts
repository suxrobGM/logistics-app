import { Pipe, type PipeTransform } from "@angular/core";

type RelativeTimeStyle = "long" | "short" | "narrow";

/**
 * Pipe to display relative time (e.g., "3 minutes ago", "in 2 hours").
 * Supports 'long', 'short', and 'narrow' styles.
 * Avialable formats:
 * - long: "3 minutes ago", "in 2 hours"
 * - short: "3 min. ago", "in 2 hr."
 * - narrow: "3m ago", "in 2h"
 */
@Pipe({
  name: "relativeTime",
})
export class RelativeTimePipe implements PipeTransform {
  private readonly formatters = new Map<string, Intl.RelativeTimeFormat>();

  transform(
    value: Date | string | number | null | undefined,
    style: RelativeTimeStyle = "long",
  ): string {
    if (value == null) {
      return "";
    }

    const date = this.toDate(value);
    if (!date || isNaN(date.getTime())) {
      return "";
    }

    const now = new Date();
    const diffMs = date.getTime() - now.getTime();
    const diffSeconds = Math.round(diffMs / 1000);

    // "Just now" for very recent times (within 30 seconds)
    if (Math.abs(diffSeconds) < 30) {
      return "Just now";
    }

    const formatter = this.getFormatter(style);
    const { value: timeValue, unit } = this.getTimeUnit(diffSeconds);

    return formatter.format(timeValue, unit);
  }

  private toDate(value: Date | string | number): Date | null {
    if (value instanceof Date) return value;
    if (typeof value === "number") return new Date(value);
    if (typeof value === "string") {
      const parsed = new Date(value);
      return isNaN(parsed.getTime()) ? null : parsed;
    }
    return null;
  }

  private getFormatter(style: RelativeTimeStyle): Intl.RelativeTimeFormat {
    if (!this.formatters.has(style)) {
      this.formatters.set(style, new Intl.RelativeTimeFormat("en", { numeric: "auto", style }));
    }
    return this.formatters.get(style)!;
  }

  private getTimeUnit(diffSeconds: number): {
    value: number;
    unit: Intl.RelativeTimeFormatUnit;
  } {
    const absDiff = Math.abs(diffSeconds);
    const sign = diffSeconds < 0 ? -1 : 1;

    const minute = 60;
    const hour = minute * 60;
    const day = hour * 24;
    const week = day * 7;
    const month = day * 30;
    const year = day * 365;

    if (absDiff < minute) {
      return { value: sign * Math.round(absDiff), unit: "second" };
    }
    if (absDiff < hour) {
      return { value: sign * Math.round(absDiff / minute), unit: "minute" };
    }
    if (absDiff < day) {
      return { value: sign * Math.round(absDiff / hour), unit: "hour" };
    }
    if (absDiff < week) {
      return { value: sign * Math.round(absDiff / day), unit: "day" };
    }
    if (absDiff < month) {
      return { value: sign * Math.round(absDiff / week), unit: "week" };
    }
    if (absDiff < year) {
      return { value: sign * Math.round(absDiff / month), unit: "month" };
    }
    return { value: sign * Math.round(absDiff / year), unit: "year" };
  }
}
