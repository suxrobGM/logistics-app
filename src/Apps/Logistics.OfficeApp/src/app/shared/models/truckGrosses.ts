import { DailyGrosses } from "./dailyGrosses";

export interface TruckGrosses {
  truckId: string;
  grosses?: DailyGrosses;
  incomeAllTime: number;
  distanceAllTime: number;
}