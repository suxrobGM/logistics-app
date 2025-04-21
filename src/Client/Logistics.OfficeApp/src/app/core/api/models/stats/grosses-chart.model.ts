import {DailyGrossDto} from "./daily-gross.model";

export interface GrossesChartDto<T extends DailyGrossDto> {
  data: T[];
  totalGross: number;
  totalDistance: number;
}
