import type { Tag } from "primeng/tag";

export function getPerformanceLevel(efficiency: number): string {
  if (efficiency >= 2.0) return "Excellent";
  if (efficiency >= 1.5) return "Good";
  if (efficiency >= 1.0) return "Average";
  return "Below Average";
}

export function getPerformanceSeverity(efficiency: number): Tag["severity"] {
  if (efficiency >= 2.0) return "success";
  if (efficiency >= 1.5) return "info";
  if (efficiency >= 1.0) return "warn";
  return "danger";
}
