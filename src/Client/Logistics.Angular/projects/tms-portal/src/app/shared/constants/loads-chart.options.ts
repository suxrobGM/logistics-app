// Loads chart configs
import { getChartColors } from "./financials-chart.options";

// Colors
export const LOADS_CHART_PALETTE = [
  "#3b82f6", // Blue
  "#22c55e", // Green
  "#f59e0b", // Amber
  "#8b5cf6", // Purple
  "#ef4444", // Red
  "#06b6d4", // Cyan
  "#f97316", // Orange
];

export function getLoadsPieOptions(isDark = true) {
  const colors = getChartColors(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    aspectRatio: 1,
    plugins: {
      legend: {
        position: "bottom" as const,
        labels: {
          color: colors.textColor,
          usePointStyle: true,
          boxWidth: 8,
        },
      },
      tooltip: {
        backgroundColor: colors.tooltipBg,
        titleColor: isDark ? "#f1f5f9" : "#0f172a",
        bodyColor: colors.textColor,
        borderColor: colors.tooltipBorder,
        borderWidth: 1,
        cornerRadius: 8,
        padding: 12,
        callbacks: {
          label: (ctx: { label: string; parsed: number }) => `${ctx.label}: ${ctx.parsed}`,
        },
      },
    },
    layout: { padding: 0 },
  };
}

export function getLoadsTrendChartOptions(isDark = true) {
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
          text: "Load Count",
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
          text: "Revenue ($)",
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

export function getLoadsTypeChartOptions(isDark = true) {
  const colors = getChartColors(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: "bottom" as const,
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
        beginAtZero: true,
        ticks: { color: colors.textColor },
        grid: { color: colors.gridColor },
      },
      x: {
        ticks: { color: colors.textColor },
        grid: { color: colors.gridColor },
      },
    },
  };
}

export function getLoadsPerformanceChartOptions(isDark = true) {
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
