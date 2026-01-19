import { CommonModule } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { Api, getExpenseStats } from "@logistics/shared/api";
import type { ExpenseStatsDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { DatePicker } from "primeng/datepicker";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { PageHeader } from "@/shared/components";

@Component({
  selector: "app-expense-analytics",
  templateUrl: "./expense-analytics.html",
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardModule,
    ButtonModule,
    ChartModule,
    DatePicker,
    TableModule,
    ProgressSpinnerModule,
    PageHeader,
  ],
})
export class ExpenseAnalyticsPage implements OnInit {
  private readonly api = inject(Api);

  readonly isLoading = signal(false);
  readonly stats = signal<ExpenseStatsDto | null>(null);
  readonly fromDate = signal<Date | null>(null);
  readonly toDate = signal<Date | null>(null);

  // Chart data
  readonly typeChartData = signal<unknown>(null);
  readonly companyCategoryChartData = signal<unknown>(null);
  readonly truckCategoryChartData = signal<unknown>(null);
  readonly monthlyTrendChartData = signal<unknown>(null);

  readonly chartOptions = {
    plugins: {
      legend: {
        position: "bottom",
      },
    },
    responsive: true,
    maintainAspectRatio: false,
  };

  readonly lineChartOptions = {
    plugins: {
      legend: {
        display: false,
      },
    },
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

  ngOnInit(): void {
    // Set default date range to last 12 months
    const today = new Date();
    const lastYear = new Date();
    lastYear.setFullYear(today.getFullYear() - 1);
    this.fromDate.set(lastYear);
    this.toDate.set(today);
    this.loadStats();
  }

  async loadStats(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getExpenseStats, {
      FromDate: this.fromDate()?.toISOString(),
      ToDate: this.toDate()?.toISOString(),
    });

    if (result) {
      this.stats.set(result);
      this.updateCharts(result);
    }

    this.isLoading.set(false);
  }

  onDateChange(): void {
    this.loadStats();
  }

  exportToCsv(): void {
    const s = this.stats();
    if (!s) return;

    const rows = [
      ["Expense Analytics Report"],
      ["Generated:", new Date().toISOString()],
      [],
      ["Summary"],
      ["Total Amount", s.totalAmount],
      ["Total Count", s.totalCount],
      ["Pending Amount", s.pendingAmount],
      ["Approved Amount", s.approvedAmount],
      ["Paid Amount", s.paidAmount],
      [],
      ["By Type"],
      ["Type", "Amount", "Count"],
      ...(s.byType ?? []).map((t) => [t.type, t.amount, t.count]),
      [],
      ["By Company Category"],
      ["Category", "Amount", "Count"],
      ...(s.byCompanyCategory ?? []).map((c) => [c.category, c.amount, c.count]),
      [],
      ["By Truck Category"],
      ["Category", "Amount", "Count"],
      ...(s.byTruckCategory ?? []).map((c) => [c.category, c.amount, c.count]),
      [],
      ["Top Trucks"],
      ["Truck", "Amount", "Count"],
      ...(s.topTrucks ?? []).map((t) => [t.truckNumber, t.totalAmount, t.expenseCount]),
    ];

    const csvContent = rows.map((row) => row.join(",")).join("\n");
    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `expense-analytics-${new Date().toISOString().split("T")[0]}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  private updateCharts(s: ExpenseStatsDto): void {
    const byType = s.byType ?? [];
    const byCompanyCategory = s.byCompanyCategory ?? [];
    const byTruckCategory = s.byTruckCategory ?? [];
    const monthlyTrend = s.monthlyTrend ?? [];

    // Type distribution pie chart
    if (byType.length > 0) {
      this.typeChartData.set({
        labels: byType.map((t) => t.type),
        datasets: [
          {
            data: byType.map((t) => t.amount),
            backgroundColor: ["#3B82F6", "#10B981", "#F59E0B"],
          },
        ],
      });
    }

    // Company category pie chart
    if (byCompanyCategory.length > 0) {
      this.companyCategoryChartData.set({
        labels: byCompanyCategory.map((c) => c.category),
        datasets: [
          {
            data: byCompanyCategory.map((c) => c.amount),
            backgroundColor: ["#6366F1", "#8B5CF6", "#A855F7", "#D946EF", "#EC4899", "#F43F5E"],
          },
        ],
      });
    }

    // Truck category bar chart
    if (byTruckCategory.length > 0) {
      this.truckCategoryChartData.set({
        labels: byTruckCategory.map((c) => c.category),
        datasets: [
          {
            label: "Amount",
            data: byTruckCategory.map((c) => c.amount),
            backgroundColor: "#10B981",
          },
        ],
      });
    }

    // Monthly trend line chart
    if (monthlyTrend.length > 0) {
      const monthNames = [
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

      this.monthlyTrendChartData.set({
        labels: monthlyTrend.map((m) => `${monthNames[(m.month ?? 1) - 1]} ${m.year}`),
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
      });
    }
  }
}
