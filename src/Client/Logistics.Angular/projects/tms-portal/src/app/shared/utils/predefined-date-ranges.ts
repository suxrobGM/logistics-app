import { DateRange } from "./date-range";

export type DatePreset = "today" | "week" | "month";

/**
 * Returns a date range tuple [startDate, endDate] for the given preset.
 * The start date is normalized to 00:00:00.000 and end date to 23:59:59.999.
 * Suitable for use with PrimeNG datepicker in range mode.
 */
export function getDatePreset(preset: DatePreset): [Date, Date] {
  const today = new Date();
  let start: Date;
  let end: Date;

  switch (preset) {
    case "today":
      start = new Date(today);
      start.setHours(0, 0, 0, 0);
      end = new Date(today);
      end.setHours(23, 59, 59, 999);
      break;
    case "week": {
      start = new Date(today);
      start.setDate(today.getDate() - today.getDay());
      start.setHours(0, 0, 0, 0);
      end = new Date(start);
      end.setDate(start.getDate() + 6);
      end.setHours(23, 59, 59, 999);
      break;
    }
    case "month": {
      start = new Date(today.getFullYear(), today.getMonth(), 1);
      start.setHours(0, 0, 0, 0);
      end = new Date(today.getFullYear(), today.getMonth() + 1, 0);
      end.setHours(23, 59, 59, 999);
      break;
    }
  }

  return [start, end];
}

export abstract class PredefinedDateRanges {
  static getThisWeek(): DateRange {
    const start = new Date();
    start.setDate(start.getDate() - start.getDay() + 1);
    const end = new Date(start);
    end.setDate(start.getDate() + 6);
    return new DateRange("This Week", start, end);
  }

  static getLastWeek(): DateRange {
    const start = new Date();
    start.setDate(start.getDate() - start.getDay() - 6);
    const end = new Date(start);
    end.setDate(start.getDate() + 6);
    return new DateRange("Last Week", start, end);
  }

  static getPastTwoWeeks(): DateRange {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 14);
    return new DateRange("Past Two Weeks", start, end);
  }

  static getThisMonth(): DateRange {
    const start = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    const end = new Date();
    return new DateRange("This Month", start, end);
  }

  static getLastMonth(): DateRange {
    const start = new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1);
    const end = new Date(new Date().getFullYear(), new Date().getMonth(), 0);
    return new DateRange("Last Month", start, end);
  }

  static getPast90Days(): DateRange {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 90);
    return new DateRange("Past 90 Days", start, end);
  }

  static getThisYear(): DateRange {
    const start = new Date(new Date().getFullYear(), 0, 1);
    const end = new Date();
    return new DateRange("This Year", start, end);
  }

  static getLastYear(): DateRange {
    const start = new Date(new Date().getFullYear() - 1, 0, 1);
    const end = new Date(new Date().getFullYear() - 1, 11, 31);
    return new DateRange("Last Year", start, end);
  }
}
