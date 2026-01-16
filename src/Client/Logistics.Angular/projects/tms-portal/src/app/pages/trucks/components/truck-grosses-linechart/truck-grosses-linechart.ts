import { DatePipe } from "@angular/common";
import { Component, type OnInit, inject, input, model, output, signal } from "@angular/core";
import { Api, getDailyGrosses } from "@logistics/shared/api";
import type { DailyGrossesDto } from "@logistics/shared/api/models";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { RangeCalendar } from "@/shared/components";
import { Converters, DateUtils } from "@/shared/utils";

const chartInitialData = {
  labels: [],
  datasets: [
    {
      label: "Daily Gross",
      data: [],
    },
  ],
};

const chartOptions = {
  plugins: {
    legend: {
      display: false,
    },
  },
};

@Component({
  selector: "app-truck-grosses-linechart",
  templateUrl: "./truck-grosses-linechart.html",
  imports: [DatePipe, CardModule, SkeletonModule, ChartModule, RangeCalendar, DividerModule],
})
export class TruckGrossesLinechartComponent implements OnInit {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly dailyGrosses = signal<DailyGrossesDto | null>(null);
  protected readonly chartData = signal<Record<string, unknown>>(chartInitialData);

  protected readonly chartOptions = signal<Record<string, unknown>>(chartOptions);
  protected readonly startDate = model(DateUtils.daysAgo(30));
  protected readonly endDate = model(DateUtils.today());

  public readonly truckId = input.required<string>();
  public readonly chartDrawn = output<LineChartDrawnEvent>();

  ngOnInit(): void {
    this.fetchDailyGrosses();
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
      const rpm = (result.totalGross ?? 0) / Converters.metersTo(result.totalDistance ?? 0, "mi");

      this.drawChart(result);
      this.chartDrawn.emit({ dailyGrosses: result, rpm: rpm });
    }

    this.isLoading.set(false);
  }

  private drawChart(grosses: DailyGrossesDto): void {
    const labels: string[] = [];
    const data: number[] = [];

    (grosses.data ?? []).forEach((i) => {
      labels.push(DateUtils.toLocaleDate(i.date ?? ""));
      data.push(i.gross ?? 0);
    });

    this.chartData.set({
      labels: labels,
      datasets: [
        {
          label: "Daily Gross",
          data: data,
          fill: true,
          tension: 0.4,
          borderColor: "#405a83",
          backgroundColor: "#88a5d3",
        },
      ],
    });
  }
}

export interface LineChartDrawnEvent {
  dailyGrosses: DailyGrossesDto;
  rpm: number;
}
