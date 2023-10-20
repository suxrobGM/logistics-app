import {DailyGross} from './dailyGross';

export interface GrossesChart<T extends DailyGross> {
  data: T[];
  totalGross: number;
  totalDistance: number;
}
