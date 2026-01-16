export const DRIVERS_CHART_PALETTE = [
  "#2563eb",
  "#16a34a",
  "#f59e0b",
  "#a855f7",
  "#ef4444",
  "#06b6d4",
  "#f97316",
];

export const DRIVERS_TREND_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { position: "top" } },
  scales: {
    y: {
      type: "linear",
      display: true,
      position: "left",
      title: { display: true, text: "Active Drivers" },
    },
    y1: {
      type: "linear",
      display: true,
      position: "right",
      title: { display: true, text: "Loads Delivered" },
      grid: { drawOnChartArea: false },
    },
  },
};

export const DRIVERS_PERFORMANCE_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: {
    y: { beginAtZero: true, title: { display: true, text: "Earnings ($)" } },
  },
};

export const DRIVERS_EFFICIENCY_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: { y: { beginAtZero: true } },
};
