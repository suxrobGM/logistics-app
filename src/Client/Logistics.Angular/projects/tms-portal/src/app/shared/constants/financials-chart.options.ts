// Financials chart configs — sourced from `chart-palette.ts`
import {
  getChartPalette,
  INVOICE_STATUS_COLORS,
  INVOICE_STATUS_HOVER_COLORS,
} from "./chart-palette";

export const FINANCIALS_CHART_LABELS = ["Fully Paid", "Partially Paid", "Unpaid"];
export const FINANCIALS_CHART_BACKGROUND_COLORS = INVOICE_STATUS_COLORS;
export const FINANCIALS_CHART_HOVER_BACKGROUND_COLORS = INVOICE_STATUS_HOVER_COLORS;

export function getFinancialMetricsChartOptions(isDark = true) {
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

export function getRevenueTrendChartOptions(isDark = true) {
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
          text: "Amount ($)",
          color: p.textColor,
        },
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

export function getInvoiceStatusChartOptions(isDark = true) {
  const p = getChartPalette(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    aspectRatio: 1,
    plugins: {
      legend: {
        position: "bottom" as const,
        labels: {
          usePointStyle: true,
          boxWidth: 8,
          color: p.textColor,
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
