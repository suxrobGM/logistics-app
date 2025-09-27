import { CommonModule } from "@angular/common";
import { Component, OnInit, inject, input, output, signal } from "@angular/core";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { ApiService } from "@/core/api";
import { MonthlyGrossesDto } from "@/core/api/models";
import { Converters, DateUtils } from "@/shared/utils";
import { RangeCalendar } from "../range-calendar/range-calendar";

const chartInitialData = {
  labels: [],
  datasets: [
    {
      label: "Monthly Gross",
      data: [],
    },
  ],
};

const chartInitialOptions = {
  plugins: {
    legend: {
      display: false,
    },
  },
};

@Component({
  selector: "app-grosses-barchart",
  standalone: true,
  templateUrl: "./grosses-barchart.html",
  styleUrls: [],
  imports: [CommonModule, CardModule, SkeletonModule, ChartModule, RangeCalendar],
})
export class GrossesBarchart implements OnInit {
  private readonly apiService = inject(ApiService);
  protected readonly isLoading = signal(false);
  protected readonly monthlyGrosses = signal<MonthlyGrossesDto | null>(null);
  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());
  protected readonly chartData = signal<Record<string, unknown>>(chartInitialData);
  protected readonly chartOptions = signal<Record<string, unknown>>(chartInitialOptions);

  public readonly truckId = input<string>();
  public readonly chartColor = input<string>("#EC407A");
  public readonly chartDrawn = output<BarChartDrawnEvent>();

  constructor() {
    this.fetchMonthlyGrosses();
  }

  ngOnInit(): void {
    this.fetchMonthlyGrosses();
  }

  fetchMonthlyGrosses() {
    this.isLoading.set(true);

    this.apiService
      .getMonthlyGrosses(this.startDate(), this.endDate(), this.truckId())
      .subscribe((result) => {
        if (result.success && result.data) {
          const monthlyGrosses = result.data;
          this.monthlyGrosses.set(monthlyGrosses);
          const rpm =
            monthlyGrosses.totalGross / Converters.metersTo(monthlyGrosses.totalDistance, "mi");

          this.drawChart(monthlyGrosses);
          this.chartDrawn.emit({ monthlyGrosses, rpm });
        }

        this.isLoading.set(false);
      });
  }

  private drawChart(grosses: MonthlyGrossesDto) {
    const labels: string[] = [];
    const data: number[] = [];

    grosses.data.forEach((i) => {
      labels.push(DateUtils.monthNameWithYear(i.date));
      data.push(i.gross);
    });

    this.chartData.set({
      labels: labels,
      datasets: [
        {
          label: "Monthly Gross",
          data: data,
          fill: true,
          tension: 0.4,
          backgroundColor: this.chartColor(),
        },
      ],
    });
  }
}

export interface BarChartDrawnEvent {
  monthlyGrosses: MonthlyGrossesDto;
  rpm: number;
}
