import {DailyGross} from './dailyGross';

export interface DailyGrosses {
  dates: DailyGross[];
  totalIncome: number;
  totalDistance: number;
}
