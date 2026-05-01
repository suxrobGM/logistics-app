/**
 * Chart palette — single source of truth for chart colors across the TMS portal.
 *
 * All chart-options files (loads, drivers, financials, payroll) and chart components
 * (daily-gross-chart, gross-barchart) should source colors from here, not from
 * inline hex literals.
 *
 * Color values mirror the `--status-*` and `--success`/`--warning`/`--danger`/`--info`
 * tokens in `tms-portal/src/styles/variables.css`. When the design tokens change,
 * update the constants below to keep chart slices visually consistent with badges.
 */

export interface ChartPalette {
  /** Primary axis tick / legend / title color */
  textColor: string;
  /** Gridline color (subtle) */
  gridColor: string;
  /** Tooltip background */
  tooltipBg: string;
  /** Tooltip border */
  tooltipBorder: string;
  /** Tooltip title color (slightly stronger than textColor) */
  titleColor: string;
  /** Brand primary chart color (line/bar default) */
  primaryColor: string;
  /** Gradient stop for primary fill (top — most opaque) */
  primaryColorGradientStart: string;
  /** Gradient stop for primary fill (bottom — most transparent) */
  primaryColorGradientEnd: string;
}

/**
 * Brand primary color, matching `--primary-*` tokens in variables.css.
 * Light mode uses cyan-600; dark mode uses cyan-500 for better contrast.
 */
export const CHART_PRIMARY_COLOR_LIGHT = "#0891b2";
export const CHART_PRIMARY_COLOR_DARK = "#06b6d4";

/**
 * Categorical palette — used for multi-series charts (loads breakdown, drivers, etc.).
 * Aligned with status tokens so chart slices match adjacent status badges.
 * Order: info, success, warning, pickedup, danger, completed, orange, neutral.
 */
export const CHART_CATEGORICAL: readonly string[] = [
  "#3b82f6", // blue (info / dispatched)
  "#22c55e", // green (success / delivered)
  "#f59e0b", // amber (warning / pending)
  "#8b5cf6", // violet (pickedup)
  "#ef4444", // red (danger / cancelled)
  "#06b6d4", // cyan (completed / primary)
  "#f97316", // orange
  "#64748b", // slate (neutral)
];

/**
 * Semantic status colors (light + dark variants).
 * Mirrors `--success`, `--warning`, `--danger`, `--info` in variables.css.
 */
export const CHART_STATUS_COLORS = {
  light: {
    success: "#16a34a",
    warning: "#d97706",
    danger: "#dc2626",
    info: "#2563eb",
  },
  dark: {
    success: "#22c55e",
    warning: "#f59e0b",
    danger: "#ef4444",
    info: "#3b82f6",
  },
} as const;

/**
 * Invoice status colors (used by financials doughnut chart).
 * "Fully Paid" → success, "Partially Paid" → warning, "Unpaid" → danger.
 */
export const INVOICE_STATUS_COLORS = ["#22c55e", "#f59e0b", "#ef4444"];
export const INVOICE_STATUS_HOVER_COLORS = ["#16a34a", "#d97706", "#dc2626"];

/**
 * Theme-aware chart palette. Returns canonical color set used by tooltips,
 * axes, gridlines, and the brand primary line/bar default.
 */
export function getChartPalette(isDark: boolean): ChartPalette {
  return {
    textColor: isDark ? "#94a3b8" : "#475569",
    gridColor: isDark ? "rgba(45, 53, 72, 0.5)" : "rgba(203, 213, 225, 0.5)",
    tooltipBg: isDark ? "#1a1f2e" : "#ffffff",
    tooltipBorder: isDark ? "#3d4760" : "#e2e8f0",
    titleColor: isDark ? "#f1f5f9" : "#0f172a",
    primaryColor: isDark ? CHART_PRIMARY_COLOR_DARK : CHART_PRIMARY_COLOR_LIGHT,
    primaryColorGradientStart: isDark ? "rgba(6, 182, 212, 0.3)" : "rgba(8, 145, 178, 0.25)",
    primaryColorGradientEnd: isDark ? "rgba(6, 182, 212, 0.02)" : "rgba(8, 145, 178, 0.02)",
  };
}

/**
 * Map a domain status string to its chart slice color, matching `StatusBadge` colors.
 * Use this when a chart's slices represent the same statuses shown in adjacent tables,
 * so the green slice is the same green as the "Delivered" / "Fully Paid" badge.
 */
export function getStatusColor(status: string, isDark: boolean): string {
  const tokens = isDark ? CHART_STATUS_COLORS.dark : CHART_STATUS_COLORS.light;
  const key = status.toLowerCase();

  // Load lifecycle
  if (key === "delivered" || key === "completed") return tokens.success;
  if (key === "dispatched") return tokens.info;
  if (key === "pickedup" || key === "picked_up") return isDark ? "#8b5cf6" : "#7c3aed";
  if (key === "cancelled" || key === "canceled") return tokens.danger;
  if (key === "pending" || key === "draft") return tokens.warning;

  // Invoice lifecycle
  if (key === "fullypaid" || key === "fully_paid" || key === "paid") return tokens.success;
  if (key === "partiallypaid" || key === "partially_paid") return tokens.warning;
  if (key === "unpaid" || key === "overdue") return tokens.danger;

  // Truck status
  if (key === "available" || key === "active") return tokens.success;
  if (key === "idle") return tokens.warning;
  if (key === "outofservice" || key === "out_of_service") return tokens.danger;

  return CHART_CATEGORICAL[5]; // cyan fallback
}

interface ChartGradientContext {
  chart: {
    ctx: CanvasRenderingContext2D;
    chartArea: { top: number; bottom: number };
  };
}

/**
 * Build a vertical area-fill gradient for line charts (most opaque at top, fades to nearly transparent).
 * Pass directly to dataset `backgroundColor` as a function:
 *   backgroundColor: (ctx) => getLineGradient(ctx, isDark)
 */
export function getLineGradient(
  context: ChartGradientContext,
  isDark: boolean,
): CanvasGradient | string {
  const ctx = context.chart.ctx;
  const chartArea = context.chart.chartArea;
  const palette = getChartPalette(isDark);
  if (!chartArea) {
    return palette.primaryColorGradientStart;
  }

  const gradient = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
  gradient.addColorStop(0, palette.primaryColorGradientStart);
  gradient.addColorStop(1, palette.primaryColorGradientEnd);
  return gradient;
}

/**
 * Build a vertical bar-fill gradient (top-down: lighter → fuller).
 * Useful for accent bars where a single color feels flat.
 */
export function getBarGradient(
  context: ChartGradientContext,
  isDark: boolean,
  baseColor?: string,
): CanvasGradient | string {
  const ctx = context.chart.ctx;
  const chartArea = context.chart.chartArea;
  const color = baseColor ?? getChartPalette(isDark).primaryColor;
  if (!chartArea) return color;

  const gradient = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
  gradient.addColorStop(0, adjustAlpha(color, 0.95));
  gradient.addColorStop(1, adjustAlpha(color, 0.65));
  return gradient;
}

/** Apply alpha to a `#RRGGBB` color, returning `rgba(r,g,b,a)`. */
function adjustAlpha(hex: string, alpha: number): string {
  if (!hex.startsWith("#") || hex.length !== 7) return hex;
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}
