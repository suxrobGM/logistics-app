import { CurrencyPipe } from "@angular/common";
import { Component, type OnInit, computed, inject, output, signal } from "@angular/core";
import { Api, getDailyGrosses } from "@logistics/shared/api";
import type { DailyGrossesDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { Converters, DateUtils } from "@/shared/utils";

export interface DailyGrossChartData {
  totalGross: number;
  totalDistance: number;
  rpm: number;
  todayGross: number;
}

@Component({
  selector: "app-daily-gross-chart",
  templateUrl: "./daily-gross-chart.html",
  imports: [CardModule, ChartModule, DividerModule, SkeletonModule, CurrencyPipe],
})
export class DailyGrossChartComponent implements OnInit {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly dailyGrosses = signal<DailyGrossesDto | null>(null);
  protected readonly chartData = signal<Record<string, unknown>>({ labels: [], datasets: [] });

  public readonly dataLoaded = output<DailyGrossChartData>();

  protected readonly totalGross = computed(() => this.dailyGrosses()?.totalGross ?? 0);
  protected readonly totalDistance = computed(() =>
    Converters.metersTo(this.dailyGrosses()?.totalDistance ?? 0, "mi"),
  );
  protected readonly rpm = computed(() => {
    const distance = this.totalDistance();
    return distance > 0 ? this.totalGross() / distance : 0;
  });
  protected readonly todayGross = computed(() => {
    const today = new Date();
    let total = 0;
    (this.dailyGrosses()?.data ?? [])
      .filter((i) => i.date && DateUtils.dayOfMonth(i.date) === today.getDate())
      .forEach((i) => (total += i.gross ?? 0));
    return total;
  });

  protected readonly chartOptions = computed(() => ({
    maintainAspectRatio: false,
    responsive: true,
    interaction: {
      intersect: false,
      mode: "index",
    },
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        backgroundColor: "rgba(15, 23, 42, 0.9)",
        titleFont: { size: 13, weight: "600" },
        bodyFont: { size: 12 },
        padding: 12,
        cornerRadius: 8,
        displayColors: false,
        callbacks: {
          label: (context: { parsed: { y: number } }) => {
            const value = context.parsed.y;
            return ` $${value.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
          },
        },
      },
    },
    scales: {
      x: {
        grid: {
          display: false,
        },
        ticks: {
          font: { size: 11 },
          color: "#64748b",
        },
        border: {
          display: false,
        },
      },
      y: {
        beginAtZero: true,
        grid: {
          color: "rgba(148, 163, 184, 0.1)",
        },
        ticks: {
          font: { size: 11 },
          color: "#64748b",
          callback: (value: number) => `$${value.toLocaleString()}`,
        },
        border: {
          display: false,
        },
      },
    },
  }));

  ngOnInit(): void {
    this.fetchDailyGrosses();
  }

  async fetchDailyGrosses(): Promise<void> {
    this.isLoading.set(true);
    const oneWeekAgo = DateUtils.daysAgo(7);

    const result = await this.api.invoke(getDailyGrosses, {
      StartDate: oneWeekAgo.toISOString(),
    });

    if (result) {
      this.dailyGrosses.set(result);
      this.drawChart(result);
      this.dataLoaded.emit({
        totalGross: this.totalGross(),
        totalDistance: this.dailyGrosses()?.totalDistance ?? 0,
        rpm: this.rpm(),
        todayGross: this.todayGross(),
      });
    }

    this.isLoading.set(false);
  }

  private drawChart(grosses: DailyGrossesDto): void {
    const labels: string[] = [];
    const data: number[] = [];

    (grosses.data ?? []).forEach((i) => {
      if (i.date) {
        const date = new Date(i.date);
        labels.push(
          date.toLocaleDateString("en-US", { weekday: "short", month: "short", day: "numeric" }),
        );
        data.push(i.gross ?? 0);
      }
    });

    // Create gradient effect
    this.chartData.set({
      labels,
      datasets: [
        {
          label: "Daily Gross",
          data,
          fill: true,
          tension: 0.4,
          borderColor: "#06b6d4",
          borderWidth: 2,
          backgroundColor: (context: {
            chart: { ctx: CanvasRenderingContext2D; chartArea: { top: number; bottom: number } };
          }) => {
            const ctx = context.chart.ctx;
            const chartArea = context.chart.chartArea;
            if (!chartArea) return "rgba(6, 182, 212, 0.1)";

            const gradient = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
            gradient.addColorStop(0, "rgba(6, 182, 212, 0.3)");
            gradient.addColorStop(1, "rgba(6, 182, 212, 0.02)");
            return gradient;
          },
          pointBackgroundColor: "#06b6d4",
          pointBorderColor: "#fff",
          pointBorderWidth: 2,
          pointRadius: 4,
          pointHoverRadius: 6,
          pointHoverBackgroundColor: "#06b6d4",
          pointHoverBorderColor: "#fff",
          pointHoverBorderWidth: 2,
        },
      ],
    });
  }
}
