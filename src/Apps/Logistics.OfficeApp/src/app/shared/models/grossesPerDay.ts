export interface GrossesPerDay {
  days: DailyGross[];
  totalGross: number;
}

export interface DailyGross {
  day: string;
  gross: number;
}