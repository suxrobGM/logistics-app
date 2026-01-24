// Financials chart configs

export const FINANCIALS_CHART_LABELS = ["Fully Paid", "Partially Paid", "Unpaid"];
export const FINANCIALS_CHART_BACKGROUND_COLORS = ["#22c55e", "#f59e0b", "#ef4444"];
export const FINANCIALS_CHART_HOVER_BACKGROUND_COLORS = ["#16a34a", "#d97706", "#dc2626"];

// Theme-aware chart colors helper
export function getChartColors(isDark: boolean) {
  return {
    textColor: isDark ? "#94a3b8" : "#475569",
    gridColor: isDark ? "rgba(45, 53, 72, 0.5)" : "rgba(203, 213, 225, 0.5)",
    tooltipBg: isDark ? "#1a1f2e" : "#ffffff",
    tooltipBorder: isDark ? "#3d4760" : "#e2e8f0",
  };
}

export function getFinancialMetricsChartOptions(isDark = true) {
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

export function getRevenueTrendChartOptions(isDark = true) {
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
          text: "Amount ($)",
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

export function getInvoiceStatusChartOptions(isDark = true) {
  const colors = getChartColors(isDark);
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
          color: colors.textColor,
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

// Legacy exports for backward compatibility
export const FINANCIAL_METRICS_CHART_OPTIONS = getFinancialMetricsChartOptions(true);
export const REVENUE_TREND_CHART_OPTIONS = getRevenueTrendChartOptions(true);
export const INVOICE_STATUS_CHART_OPTIONS = getInvoiceStatusChartOptions(true);
