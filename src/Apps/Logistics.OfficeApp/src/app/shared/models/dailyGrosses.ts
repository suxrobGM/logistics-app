import { DailyGross } from './dailyGross';

export interface DailyGrosses {
  days: DailyGross[];
  totalGross: number;
  totalDistance: number;
}
