import { DailyGross } from './dailyGross';

export interface DailyGrosses {
  days: DailyGross[];
  totalIncome: number;
  totalDistance: number;
}