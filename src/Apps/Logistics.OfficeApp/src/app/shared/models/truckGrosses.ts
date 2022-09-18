import { GrossesForInterval } from "./grossesForInterval";

export interface TruckGrosses {
  truckId: string;
  grosses?: GrossesForInterval;
  totalGrossAllTime: number;
  totalDistanceAllTime: number;
}