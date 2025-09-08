// Financials chart configs

export const FINANCIALS_CHART_LABELS = ["Fully Paid", "Partially Paid", "Unpaid"];
export const FINANCIALS_CHART_BACKGROUND_COLORS = ["#4caf50", "#ff9800", "#f44336"];
export const FINANCIALS_CHART_HOVER_BACKGROUND_COLORS = ["#66bb6a", "#ffb74d", "#e57373"];

export const FINANCIAL_METRICS_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false }
  },
  scales: {
    y: { beginAtZero: true }
  }
};

export const REVENUE_TREND_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'top' },
  },
  scales: {
    y: {
      type: 'linear',
      display: true,
      position: 'left',
      title: {
        display: true,
        text: 'Amount ($)'
      }
    }
  }
};

export const INVOICE_STATUS_CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  aspectRatio: 1,
  plugins: {
    legend: {
      position: 'bottom',
      labels: { usePointStyle: true, boxWidth: 8 }
    },
    tooltip: {
      callbacks: {
        label: (ctx: any) => `${ctx.label}: ${ctx.parsed}`
      }
    }
  },
  layout: { padding: 0 }
};
