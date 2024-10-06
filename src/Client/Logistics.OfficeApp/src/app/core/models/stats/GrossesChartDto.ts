import {DailyGrossDto} from "./DailyGrossDto";

export interface GrossesChartDto<T extends DailyGrossDto> {
  data: T[];
  totalGross: number;
  totalDistance: number;
}
