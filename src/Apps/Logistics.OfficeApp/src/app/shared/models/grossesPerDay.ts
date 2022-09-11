export interface GrossesPerDay {
  days: DailyGross[];
  totalGross: number;
  totalDistance: number;
}

export interface DailyGross {
  date: string;
  gross: number;
  distance: number;
}