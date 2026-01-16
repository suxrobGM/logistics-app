import { CurrencyPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { TrucksMap } from "@logistics/shared/components";
import { AddressPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { Converters, DateUtils } from "@logistics/shared/utils";
import { SharedModule } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { Api, getDailyGrosses, getLoads } from "@/core/api";
import type { AddressDto, DailyGrossesDto, LoadDto } from "@/core/api/models";
import { NotificationsPanelComponent } from "./components";

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
    DividerModule,
  ],
  providers: [AddressPipe],
})
export class HomeComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly addressPipe = inject(AddressPipe);

  protected readonly todayGross = signal(0);
  protected readonly weeklyGross = signal(0);
  protected readonly weeklyDistance = signal(0);
  protected readonly weeklyRpm = signal(0);
  protected readonly isLoadingLoadsData = signal(false);
  protected readonly isLoadingChartData = signal(false);
  protected readonly loads = signal<LoadDto[]>([]);
  protected readonly chartData = signal<Record<string, unknown>>(chartInitialData);
  protected readonly chartOptions = signal<Record<string, unknown>>(chartOptions);

  ngOnInit(): void {
    this.fetchActiveLoads();
    this.fetchLastTenDaysGross();
  }

  protected formatAddress(addressObj: AddressDto): string {
    return this.addressPipe.transform(addressObj) || "No address provided";
  }

  private async fetchActiveLoads(): Promise<void> {
    this.isLoadingLoadsData.set(true);

    const result = await this.api.invoke(getLoads, {
      OrderBy: "-DispatchedAt",
      OnlyActiveLoads: true,
    });
    if (result) {
      this.loads.set(result.items ?? []);
    }

    this.isLoadingLoadsData.set(false);
  }

  private async fetchLastTenDaysGross(): Promise<void> {
    this.isLoadingChartData.set(true);
    const oneWeekAgo = DateUtils.daysAgo(7);

    const result = await this.api.invoke(getDailyGrosses, {
      StartDate: oneWeekAgo.toISOString(),
    });
    if (result) {
      this.weeklyGross.set(result.totalGross ?? 0);
      this.weeklyDistance.set(result.totalDistance ?? 0);
      this.weeklyRpm.set(this.weeklyGross() / Converters.metersTo(this.weeklyDistance(), "mi"));
      this.drawChart(result);
      this.calcTodayGross(result);
    }

    this.isLoadingChartData.set(false);
  }

  private drawChart(grosses: DailyGrossesDto): void {
    const labels: string[] = [];
    const data: number[] = [];

    (grosses.data ?? []).forEach((i) => {
      if (i.date) {
        labels.push(DateUtils.toLocaleDate(i.date));
        data.push(i.gross ?? 0);
      }
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

    (grosses.data ?? [])
      .filter((i) => i.date && DateUtils.dayOfMonth(i.date) === today.getDate())
      .forEach((i) => (totalGross += i.gross ?? 0));

    this.todayGross.set(totalGross);
  }
}
