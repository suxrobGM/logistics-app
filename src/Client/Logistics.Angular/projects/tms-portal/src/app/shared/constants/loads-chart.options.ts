// Loads chart configs — sourced from `chart-palette.ts`
import { CHART_CATEGORICAL, getChartPalette } from "./chart-palette";

/**
 * Categorical palette for loads breakdown charts. Re-exported from the canonical
 * `CHART_CATEGORICAL` so loads/drivers charts share the same color order.
 */
export const LOADS_CHART_PALETTE = [...CHART_CATEGORICAL.slice(0, 7)];

export function getLoadsPieOptions(isDark = true) {
  const p = getChartPalette(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    aspectRatio: 1,
    plugins: {
      legend: {
        position: "bottom" as const,
        labels: {
          color: p.textColor,
          usePointStyle: true,
          boxWidth: 8,
        },
      },
      tooltip: {
        backgroundColor: p.tooltipBg,
        titleColor: p.titleColor,
        bodyColor: p.textColor,
        borderColor: p.tooltipBorder,
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
  const p = getChartPalette(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: "top" as const,
        labels: { color: p.textColor },
      },
      tooltip: {
        backgroundColor: p.tooltipBg,
        titleColor: p.titleColor,
        bodyColor: p.textColor,
        borderColor: p.tooltipBorder,
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
          color: p.textColor,
        },
        grid: { color: p.gridColor },
        ticks: { color: p.textColor },
      },
      y1: {
        type: "linear" as const,
        display: true,
        position: "right" as const,
        title: {
          display: true,
          text: "Revenue ($)",
          color: p.textColor,
        },
        grid: { drawOnChartArea: false },
        ticks: { color: p.textColor },
      },
      x: {
        grid: { color: p.gridColor },
        ticks: { color: p.textColor },
      },
    },
  };
}

export function getLoadsTypeChartOptions(isDark = true) {
  const p = getChartPalette(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: "bottom" as const,
        labels: { color: p.textColor },
      },
      tooltip: {
        backgroundColor: p.tooltipBg,
        titleColor: p.titleColor,
        bodyColor: p.textColor,
        borderColor: p.tooltipBorder,
        borderWidth: 1,
        cornerRadius: 8,
        padding: 12,
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { color: p.textColor },
        grid: { color: p.gridColor },
      },
      x: {
        ticks: { color: p.textColor },
        grid: { color: p.gridColor },
      },
    },
  };
}

export function getLoadsPerformanceChartOptions(isDark = true) {
  const p = getChartPalette(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: p.tooltipBg,
        titleColor: p.titleColor,
        bodyColor: p.textColor,
        borderColor: p.tooltipBorder,
        borderWidth: 1,
        cornerRadius: 8,
        padding: 12,
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        grid: { color: p.gridColor },
        ticks: { color: p.textColor },
      },
      x: {
        grid: { color: p.gridColor },
        ticks: { color: p.textColor },
      },
    },
  };
}
