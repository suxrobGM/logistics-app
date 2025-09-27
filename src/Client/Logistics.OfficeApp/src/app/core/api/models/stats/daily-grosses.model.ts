import { DailyGrossDto } from "./daily-gross.model";
import { GrossesChartDto } from "./grosses-chart.model";

export interface DailyGrossesDto extends GrossesChartDto<DailyGrossDto> {}
