import { CurrencyPipe, DecimalPipe } from "@angular/common";
import {
  Component,
  type OnInit,
  computed,
  inject,
  input,
  model,
  output,
  signal,
} from "@angular/core";
import { Api, getDailyGrosses } from "@logistics/shared/api";
import type { DailyGrossesDto } from "@logistics/shared/api";
import { hexToRgba } from "@logistics/shared/utils";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { DateRangePicker } from "@/shared/components";
import { Converters, DateUtils } from "@/shared/utils";

@Component({
  selector: "app-truck-gross-linechart",
  templateUrl: "./truck-gross-linechart.html",
  imports: [
    CardModule,
    SkeletonModule,
    ChartModule,
    DateRangePicker,
    DividerModule,
    CurrencyPipe,
    DecimalPipe,
  ],
})
export class TruckGrossLinechart implements OnInit {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly dailyGrosses = signal<DailyGrossesDto | null>(null);
  protected readonly chartData = signal<Record<string, unknown>>({ labels: [], datasets: [] });
  protected readonly startDate = model(DateUtils.daysAgo(30));
  protected readonly endDate = model(DateUtils.today());

  public readonly truckId = input.required<string>();
  public readonly chartColor = input<string>("#06b6d4");
  public readonly chartDrawn = output<LineChartDrawnEvent>();

  protected readonly totalGross = computed(() => this.dailyGrosses()?.totalGross ?? 0);
  protected readonly totalDistance = computed(() =>
    Converters.metersTo(this.dailyGrosses()?.totalDistance ?? 0, "mi"),
  );
  protected readonly rpm = computed(() => {
    const distance = this.totalDistance();
    return distance > 0 ? this.totalGross() / distance : 0;
  });
  protected readonly avgDailyGross = computed(() => {
    const data = this.dailyGrosses()?.data ?? [];
    return data.length > 0 ? this.totalGross() / data.length : 0;
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
          maxRotation: 45,
          minRotation: 0,
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

  protected onDateRangeChange(dates: Date[]): void {
    if (dates.length === 2) {
      this.startDate.set(dates[0]);
      this.endDate.set(dates[1]);
      this.fetchDailyGrosses();
    }
  }

  protected async fetchDailyGrosses(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getDailyGrosses, {
      StartDate: this.startDate().toISOString(),
      EndDate: this.endDate().toISOString(),
      TruckId: this.truckId(),
    });

    if (result) {
      this.dailyGrosses.set(result);
      this.drawChart(result);
      this.chartDrawn.emit({ dailyGrosses: result, rpm: this.rpm() });
    }

    this.isLoading.set(false);
  }

  private drawChart(grosses: DailyGrossesDto): void {
    const labels: string[] = [];
    const data: number[] = [];

    (grosses.data ?? []).forEach((i) => {
      if (i.date) {
        const date = new Date(i.date);
        labels.push(date.toLocaleDateString("en-US", { month: "short", day: "numeric" }));
        data.push(i.gross ?? 0);
      }
    });

    const color = this.chartColor();
    this.chartData.set({
      labels,
      datasets: [
        {
          label: "Daily Gross",
          data,
          fill: true,
          tension: 0.4,
          borderColor: color,
          borderWidth: 2,
          backgroundColor: (context: {
            chart: { ctx: CanvasRenderingContext2D; chartArea: { top: number; bottom: number } };
          }) => {
            const ctx = context.chart.ctx;
            const chartArea = context.chart.chartArea;
            if (!chartArea) {
              return hexToRgba(color, 0.1);
            }

            const gradient = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
            gradient.addColorStop(0, hexToRgba(color, 0.3));
            gradient.addColorStop(1, hexToRgba(color, 0.02));
            return gradient;
          },
          pointBackgroundColor: color,
          pointBorderColor: "#fff",
          pointBorderWidth: 2,
          pointRadius: 3,
          pointHoverRadius: 6,
          pointHoverBackgroundColor: color,
          pointHoverBorderColor: "#fff",
          pointHoverBorderWidth: 2,
        },
      ],
    });
  }
}

export interface LineChartDrawnEvent {
  dailyGrosses: DailyGrossesDto;
  rpm: number;
}
