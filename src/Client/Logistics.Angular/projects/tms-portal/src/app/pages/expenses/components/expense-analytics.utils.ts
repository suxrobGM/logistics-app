import type { ExpenseStatsDto } from "@logistics/shared/api";

const MONTH_NAMES = [
  "Jan",
  "Feb",
  "Mar",
  "Apr",
  "May",
  "Jun",
  "Jul",
  "Aug",
  "Sep",
  "Oct",
  "Nov",
  "Dec",
];

const TYPE_COLORS = ["#3B82F6", "#10B981", "#F59E0B"];
const COMPANY_CATEGORY_COLORS = ["#6366F1", "#8B5CF6", "#A855F7", "#D946EF", "#EC4899", "#F43F5E"];

export const chartOptions = {
  plugins: { legend: { position: "bottom" } },
  responsive: true,
  maintainAspectRatio: false,
};

export const lineChartOptions = {
  plugins: { legend: { display: false } },
  responsive: true,
  maintainAspectRatio: false,
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        callback: (value: number) => "$" + value.toLocaleString(),
      },
    },
  },
};

export function buildTypeChart(stats: ExpenseStatsDto | null): unknown {
  const byType = stats?.byType ?? [];
  if (byType.length === 0) return null;

  return {
    labels: byType.map((t) => t.type),
    datasets: [{ data: byType.map((t) => t.amount), backgroundColor: TYPE_COLORS }],
  };
}

export function buildCompanyCategoryChart(stats: ExpenseStatsDto | null): unknown {
  const byCompanyCategory = stats?.byCompanyCategory ?? [];
  if (byCompanyCategory.length === 0) return null;

  return {
    labels: byCompanyCategory.map((c) => c.category),
    datasets: [
      {
        data: byCompanyCategory.map((c) => c.amount),
        backgroundColor: COMPANY_CATEGORY_COLORS,
      },
    ],
  };
}

export function buildTruckCategoryChart(stats: ExpenseStatsDto | null): unknown {
  const byTruckCategory = stats?.byTruckCategory ?? [];
  if (byTruckCategory.length === 0) return null;

  return {
    labels: byTruckCategory.map((c) => c.category),
    datasets: [
      {
        label: "Amount",
        data: byTruckCategory.map((c) => c.amount),
        backgroundColor: "#10B981",
      },
    ],
  };
}

export function buildMonthlyTrendChart(stats: ExpenseStatsDto | null): unknown {
  const monthlyTrend = stats?.monthlyTrend ?? [];
  if (monthlyTrend.length === 0) return null;

  return {
    labels: monthlyTrend.map((m) => `${MONTH_NAMES[(m.month ?? 1) - 1]} ${m.year}`),
    datasets: [
      {
        label: "Monthly Expenses",
        data: monthlyTrend.map((m) => m.amount),
        fill: true,
        borderColor: "#3B82F6",
        backgroundColor: "rgba(59, 130, 246, 0.1)",
        tension: 0.4,
      },
    ],
  };
}

export function buildAnalyticsCsvRows(
  stats: ExpenseStatsDto,
): (string | number | null | undefined)[][] {
  return [
    ["Expense Analytics Report"],
    ["Generated:", new Date().toISOString()],
    [],
    ["Summary"],
    ["Total Amount", stats.totalAmount],
    ["Total Count", stats.totalCount],
    ["Pending Amount", stats.pendingAmount],
    ["Approved Amount", stats.approvedAmount],
    ["Paid Amount", stats.paidAmount],
    [],
    ["By Type"],
    ["Type", "Amount", "Count"],
    ...(stats.byType ?? []).map((t) => [t.type, t.amount, t.count]),
    [],
    ["By Company Category"],
    ["Category", "Amount", "Count"],
    ...(stats.byCompanyCategory ?? []).map((c) => [c.category, c.amount, c.count]),
    [],
    ["By Truck Category"],
    ["Category", "Amount", "Count"],
    ...(stats.byTruckCategory ?? []).map((c) => [c.category, c.amount, c.count]),
    [],
    ["Top Trucks"],
    ["Truck", "Amount", "Count"],
    ...(stats.topTrucks ?? []).map((t) => [t.truckNumber, t.totalAmount, t.expenseCount]),
  ];
}
