// Drivers chart configs
import { getChartColors } from "./financials-chart.options";

export const DRIVERS_CHART_PALETTE = [
  "#3b82f6", // Blue
  "#22c55e", // Green
  "#f59e0b", // Amber
  "#8b5cf6", // Purple
  "#ef4444", // Red
  "#06b6d4", // Cyan
  "#f97316", // Orange
];

export function getDriversTrendChartOptions(isDark = true) {
  const colors = getChartColors(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: "top" as const,
        labels: { color: colors.textColor },
      },
      tooltip: {
        backgroundColor: colors.tooltipBg,
        titleColor: isDark ? "#f1f5f9" : "#0f172a",
        bodyColor: colors.textColor,
        borderColor: colors.tooltipBorder,
        borderWidth: 1,
        cornerRadius: 8,
        padding: 12,
      },
    },
    scales: {
      y: {
        type: "linear" as const,
        display: true,
        position: "left" as const,
        title: {
          display: true,
          text: "Active Drivers",
          color: colors.textColor,
        },
        grid: { color: colors.gridColor },
        ticks: { color: colors.textColor },
      },
      y1: {
        type: "linear" as const,
        display: true,
        position: "right" as const,
        title: {
          display: true,
          text: "Loads Delivered",
          color: colors.textColor,
        },
        grid: { drawOnChartArea: false },
        ticks: { color: colors.textColor },
      },
      x: {
        grid: { color: colors.gridColor },
        ticks: { color: colors.textColor },
      },
    },
  };
}

export function getDriversPerformanceChartOptions(isDark = true) {
  const colors = getChartColors(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: colors.tooltipBg,
        titleColor: isDark ? "#f1f5f9" : "#0f172a",
        bodyColor: colors.textColor,
        borderColor: colors.tooltipBorder,
        borderWidth: 1,
        cornerRadius: 8,
        padding: 12,
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: "Earnings ($)",
          color: colors.textColor,
        },
        grid: { color: colors.gridColor },
        ticks: { color: colors.textColor },
      },
      x: {
        grid: { color: colors.gridColor },
        ticks: { color: colors.textColor },
      },
    },
  };
}

export function getDriversEfficiencyChartOptions(isDark = true) {
  const colors = getChartColors(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: colors.tooltipBg,
        titleColor: isDark ? "#f1f5f9" : "#0f172a",
        bodyColor: colors.textColor,
        borderColor: colors.tooltipBorder,
        borderWidth: 1,
        cornerRadius: 8,
        padding: 12,
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        grid: { color: colors.gridColor },
        ticks: { color: colors.textColor },
      },
      x: {
        grid: { color: colors.gridColor },
        ticks: { color: colors.textColor },
      },
    },
  };
}

// Legacy exports for backward compatibility
export const DRIVERS_TREND_CHART_OPTIONS = getDriversTrendChartOptions(true);
export const DRIVERS_PERFORMANCE_CHART_OPTIONS = getDriversPerformanceChartOptions(true);
export const DRIVERS_EFFICIENCY_CHART_OPTIONS = getDriversEfficiencyChartOptions(true);
