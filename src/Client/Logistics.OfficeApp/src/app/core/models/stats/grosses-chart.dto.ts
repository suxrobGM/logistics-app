import {DailyGrossDto} from "./daily-gross.dto";

export interface GrossesChartDto<T extends DailyGrossDto> {
  data: T[];
  totalGross: number;
  totalDistance: number;
}
