import { MonthlyGross } from './monthlyGross';

export interface MonthlyGrosses {
  months: MonthlyGross[];
  totalIncome: number;
  totalDistance: number;
}