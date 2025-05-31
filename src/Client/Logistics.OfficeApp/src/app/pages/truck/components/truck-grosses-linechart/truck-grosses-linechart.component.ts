import {CommonModule} from "@angular/common";
import {Component, OnInit, input, output} from "@angular/core";
import {CardModule} from "primeng/card";
import {ChartModule} from "primeng/chart";
import {SkeletonModule} from "primeng/skeleton";
import {RangeCalendarComponent} from "@/components";
import {ApiService} from "@/core/api";
import {DailyGrossesDto} from "@/core/api/models";
import {Converters, DateUtils} from "@/core/utils";

@Component({
  selector: "app-truck-grosses-linechart",
  standalone: true,
  templateUrl: "./truck-grosses-linechart.component.html",
  styleUrls: [],
  imports: [CommonModule, CardModule, SkeletonModule, ChartModule, RangeCalendarComponent],
})
export class TruckGrossesLinechartComponent implements OnInit {
  public isLoading: boolean;
  public dailyGrosses?: DailyGrossesDto;
  public chartData: unknown;
  public chartOptions: unknown;
  public startDate: Date;
  public endDate: Date;

  public readonly truckId = input.required<string>();
  public readonly chartDrawn = output<LineChartDrawnEvent>();

  constructor(private apiService: ApiService) {
    this.isLoading = false;
    this.startDate = DateUtils.daysAgo(30);
    this.endDate = DateUtils.today();

    this.chartOptions = {
      plugins: {
        legend: {
          display: false,
        },
      },
    };

    this.chartData = {
      labels: [],
      datasets: [
        {
          label: "Daily Gross",
          data: [],
        },
      ],
    };
  }

  ngOnInit(): void {
    this.fetchDailyGrosses();
  }

  fetchDailyGrosses() {
    this.isLoading = true;

    this.apiService
      .getDailyGrosses(this.startDate, this.endDate, this.truckId())
      .subscribe((result) => {
        if (result.success && result.data) {
          this.dailyGrosses = result.data;
          const rpm =
            this.dailyGrosses.totalGross /
            Converters.metersTo(this.dailyGrosses.totalDistance, "mi");

          this.drawChart(this.dailyGrosses);
          this.chartDrawn.emit({dailyGrosses: this.dailyGrosses, rpm: rpm});
        }

        this.isLoading = false;
      });
  }

  private drawChart(grosses: DailyGrossesDto) {
    const labels: string[] = [];
    const data: number[] = [];

    grosses.data.forEach((i) => {
      labels.push(DateUtils.toLocaleDate(i.date));
      data.push(i.gross);
    });

    this.chartData = {
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
    };
  }
}

export interface LineChartDrawnEvent {
  dailyGrosses: DailyGrossesDto;
  rpm: number;
}
