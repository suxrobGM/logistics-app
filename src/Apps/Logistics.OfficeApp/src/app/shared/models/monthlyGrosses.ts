import { MonthlyGross } from './monthlyGross';

export interface MonthlyGrosses {
  months: MonthlyGross[];
  totalGross: number;
  totalDistance: number;
}