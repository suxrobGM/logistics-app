import {CurrencyPipe} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {RouterLink} from "@angular/router";
import {SharedModule} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ChartModule} from "primeng/chart";
import {SkeletonModule} from "primeng/skeleton";
import {TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {DailyGrossesDto, LoadDto} from "@/core/api/models";
import {TrucksMap} from "@/shared/components";
import {AddressPipe, DistanceUnitPipe} from "@/shared/pipes";
import {Converters, DateUtils} from "@/shared/utils";
import {NotificationsPanelComponent} from "./components";

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
  selector: "app-home",
  templateUrl: "./home.html",
  styleUrl: "./home.css",
  imports: [
    CardModule,
    SharedModule,
    TableModule,
    RouterLink,
    TooltipModule,
    ButtonModule,
    SkeletonModule,
    ChartModule,
    CurrencyPipe,
    DistanceUnitPipe,
    TrucksMap,
    NotificationsPanelComponent,
    AddressPipe,
  ],
})
export class HomeComponent {
  private readonly apiService = inject(ApiService);

  protected readonly todayGross = signal(0);
  protected readonly weeklyGross = signal(0);
  protected readonly weeklyDistance = signal(0);
  protected readonly weeklyRpm = signal(0);
  protected readonly isLoadingLoadsData = signal(false);
  protected readonly isLoadingChartData = signal(false);
  protected readonly loads = signal<LoadDto[]>([]);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  protected readonly chartData = signal<Record<string, any>>(chartInitialData);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  protected readonly chartOptions = signal<Record<string, any>>(chartOptions);

  constructor() {
    this.fetchActiveLoads();
    this.fetchLastTenDaysGross();
  }

  private fetchActiveLoads(): void {
    this.isLoadingLoadsData.set(true);

    this.apiService.getLoads({orderBy: "-DispatchedDate"}, true).subscribe((result) => {
      if (result.success && result.data) {
        this.loads.set(result.data);
      }

      this.isLoadingLoadsData.set(false);
    });
  }

  private fetchLastTenDaysGross(): void {
    this.isLoadingChartData.set(true);
    const oneWeekAgo = DateUtils.daysAgo(7);

    this.apiService.getDailyGrosses(oneWeekAgo).subscribe((result) => {
      if (result.success && result.data) {
        const grosses = result.data;

        this.weeklyGross.set(grosses.totalGross);
        this.weeklyDistance.set(grosses.totalDistance);
        this.weeklyRpm.set(this.weeklyGross() / Converters.metersTo(this.weeklyDistance(), "mi"));
        this.drawChart(grosses);
        this.calcTodayGross(grosses);
      }

      this.isLoadingChartData.set(false);
    });
  }

  private drawChart(grosses: DailyGrossesDto): void {
    const labels: string[] = [];
    const data: number[] = [];

    grosses.data.forEach((i) => {
      labels.push(DateUtils.toLocaleDate(i.date));
      data.push(i.gross);
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

  private calcTodayGross(grosses: DailyGrossesDto): void {
    const today = new Date();
    let totalGross = 0;

    grosses.data
      .filter((i) => DateUtils.dayOfMonth(i.date) === today.getDate())
      .forEach((i) => (totalGross += i.gross));

    this.todayGross.set(totalGross);
  }
}
