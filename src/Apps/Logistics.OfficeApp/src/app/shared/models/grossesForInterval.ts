import { DailyGross } from "./dailyGross";

export interface GrossesForInterval {
  days: DailyGross[];
  totalGross: number;
  totalDistance: number;
}
