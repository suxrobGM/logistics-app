import {DailyGross} from './DailyGross_';

export interface GrossesChart<T extends DailyGross> {
  data: T[];
  totalGross: number;
  totalDistance: number;
}
