// Payroll chart configs

export const PAYROLL_STATUS_LABELS = [
  "Draft",
  "Pending Approval",
  "Approved",
  "Rejected",
  "Partially Paid",
  "Paid",
];
export const PAYROLL_STATUS_BACKGROUND_COLORS = [
  "#64748b", // Draft - slate
  "#f59e0b", // Pending Approval - amber
  "#22c55e", // Approved - green
  "#ef4444", // Rejected - red
  "#3b82f6", // Partially Paid - blue
  "#10b981", // Paid - emerald
];
export const PAYROLL_STATUS_HOVER_COLORS = [
  "#475569",
  "#d97706",
  "#16a34a",
  "#dc2626",
  "#2563eb",
  "#059669",
];

export const SALARY_TYPE_COLORS: Record<string, string> = {
  monthly: "#3b82f6", // blue
  weekly: "#8b5cf6", // violet
  hourly: "#06b6d4", // cyan
  share_of_gross: "#f59e0b", // amber
  rate_per_distance: "#22c55e", // green
  none: "#64748b", // slate
};

// Theme-aware chart colors helper
export function getPayrollChartColors(isDark: boolean) {
  return {
    textColor: isDark ? "#94a3b8" : "#475569",
    gridColor: isDark ? "rgba(45, 53, 72, 0.5)" : "rgba(203, 213, 225, 0.5)",
    tooltipBg: isDark ? "#1a1f2e" : "#ffffff",
    tooltipBorder: isDark ? "#3d4760" : "#e2e8f0",
  };
}

export function getPayrollStatusChartOptions(isDark = true) {
  const colors = getPayrollChartColors(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
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

export function getPayrollTrendChartOptions(isDark = true) {
  const colors = getPayrollChartColors(isDark);
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

export function getSalaryTypeChartOptions(isDark = true) {
  const colors = getPayrollChartColors(isDark);
  return {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: "y" as const,
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
        grid: { display: false },
        ticks: { color: colors.textColor },
      },
      x: {
        grid: { color: colors.gridColor },
        ticks: { color: colors.textColor },
        title: {
          display: true,
          text: "Amount ($)",
          color: colors.textColor,
        },
      },
    },
  };
}
