import type { DailyGrossesDto } from "@logistics/shared/api";
import { Converters, DateUtils } from "@/shared/utils";

const WINDOW_DAYS = 7;

export interface WeeklyGrossSummary {
  totalGross: number;
  /** Metres, as the API reports it. */
  totalDistance: number;
  ratePerMile: number;
  todayGross: number;
}

/**
 * Pinned to midnight so the KPI cards and the chart panel issue the same URL: the HTTP cache keys on
 * the serialized query string, and a millisecond-precision start date never repeats.
 */
export function weeklyGrossStartDate(): string {
  const start = DateUtils.daysAgo(WINDOW_DAYS);
  start.setHours(0, 0, 0, 0);
  return start.toISOString();
}

export function summarizeDailyGrosses(dto: DailyGrossesDto | null): WeeklyGrossSummary {
  const totalGross = dto?.totalGross ?? 0;
  const totalDistance = dto?.totalDistance ?? 0;
  const miles = Converters.metersTo(totalDistance, "mi");
  const today = new Date().getDate();

  return {
    totalGross,
    totalDistance,
    ratePerMile: miles > 0 ? totalGross / miles : 0,
    todayGross: (dto?.data ?? [])
      .filter((i) => i.date && DateUtils.dayOfMonth(i.date) === today)
      .reduce((sum, i) => sum + (i.gross ?? 0), 0),
  };
}
