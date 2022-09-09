export interface GrossesPerDay {
  days: DailyGross[];
  totalGross: number;
}

export interface DailyGross {
  date: string;
  gross: number;
}