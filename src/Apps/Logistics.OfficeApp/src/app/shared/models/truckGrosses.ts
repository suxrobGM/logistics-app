import { DailyGrosses } from "./dailyGrosses";

export interface TruckGrosses {
  truckId: string;
  grosses?: DailyGrosses;
  totalGrossAllTime: number;
  totalDistanceAllTime: number;
}