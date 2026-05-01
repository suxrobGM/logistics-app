// Payroll chart configs — sourced from `chart-palette.ts`
import { getChartPalette } from "./chart-palette";

export const PAYROLL_STATUS_LABELS = [
  "Draft",
  "Pending Approval",
  "Approved",
  "Rejected",
  "Partially Paid",
  "Paid",
];

export const SALARY_TYPE_COLORS: Record<string, string> = {
  monthly: "#3b82f6", // blue
  weekly: "#8b5cf6", // violet
  hourly: "#06b6d4", // cyan
  share_of_gross: "#f59e0b", // amber
  rate_per_distance: "#22c55e", // green
  none: "#64748b", // slate
};

export function getPayrollStatusChartOptions(isDark = true) {
  const p = getChartPalette(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
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

export function getPayrollTrendChartOptions(isDark = true) {
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

export function getSalaryTypeChartOptions(isDark = true) {
  const p = getChartPalette(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: "y" as const,
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
        grid: { display: false },
        ticks: { color: p.textColor },
      },
      x: {
        grid: { color: p.gridColor },
        ticks: { color: p.textColor },
        title: {
          display: true,
          text: "Amount ($)",
          color: p.textColor,
        },
      },
    },
  };
}
