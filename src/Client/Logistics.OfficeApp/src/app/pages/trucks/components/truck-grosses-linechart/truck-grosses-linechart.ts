import { DatePipe } from "@angular/common";
import { Component, type OnInit, inject, input, model, output, signal } from "@angular/core";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { Api, getDailyGrosses$Json } from "@/core/api";
import type { DailyGrossesDto } from "@/core/api/models";
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
  imports: [DatePipe, CardModule, SkeletonModule, ChartModule, RangeCalendar],
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

    const result = await this.api.invoke(getDailyGrosses$Json, {
      StartDate: this.startDate().toISOString(),
      EndDate: this.endDate().toISOString(),
      TruckId: this.truckId(),
    });
    if (result.success && result.data) {
      const dailyGrosses = result.data;
      this.dailyGrosses.set(dailyGrosses);
      const rpm = (dailyGrosses.totalGross ?? 0) / Converters.metersTo(dailyGrosses.totalDistance ?? 0, "mi");

      this.drawChart(dailyGrosses);
      this.chartDrawn.emit({ dailyGrosses: dailyGrosses, rpm: rpm });
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
