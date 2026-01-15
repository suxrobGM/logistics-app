// src/app/shared/config/chart.config.ts

// Colors
export const LOADS_CHART_PALETTE = [
  "#2563eb",
  "#16a34a",
  "#f59e0b",
  "#a855f7",
  "#ef4444",
  "#06b6d4",
  "#f97316",
];

function getCssVar(name: string) {
  return getComputedStyle(document.documentElement).getPropertyValue(name) || undefined;
}

export const TEXT_COLOR = getCssVar("--text-color") || "#334155";
export const GRID_COLOR = "#495057";

// Chart Options
export const LOADS_PIE_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  aspectRatio: 1,
  plugins: {
    legend: {
      position: "bottom",
      labels: { color: TEXT_COLOR, usePointStyle: true, boxWidth: 8 },
    },
    tooltip: {
      callbacks: {
        label: (ctx: { label: string; parsed: number }) => `${ctx.label}: ${ctx.parsed}`,
      },
    },
  },
  layout: { padding: 0 },
};

export const LOADS_TREND_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: "top" },
  },
  scales: {
    y: {
      type: "linear",
      display: true,
      position: "left",
      title: { display: true, text: "Load Count" },
    },
    y1: {
      type: "linear",
      display: true,
      position: "right",
      title: { display: true, text: "Revenue ($)" },
      grid: { drawOnChartArea: false },
    },
  },
};

export const LOADS_TYPE_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: "bottom",
      labels: { color: TEXT_COLOR },
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: { color: TEXT_COLOR },
      grid: { color: GRID_COLOR },
    },
    x: {
      ticks: { color: TEXT_COLOR },
      grid: { color: GRID_COLOR },
    },
  },
};

export const LOADS_PERFORMANCE_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: { y: { beginAtZero: true } },
};
