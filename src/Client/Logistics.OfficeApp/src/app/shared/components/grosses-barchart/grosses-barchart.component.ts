import {CommonModule} from "@angular/common";
import {Component, Input, OnInit, input, output} from "@angular/core";
import {CardModule} from "primeng/card";
import {ChartModule} from "primeng/chart";
import {SkeletonModule} from "primeng/skeleton";
import {ApiService} from "@/core/api";
import {MonthlyGrossesDto} from "@/core/api/models";
import {Converters, DateUtils} from "@/shared/utils";
import {RangeCalendarComponent} from "../range-calendar/range-calendar.component";

@Component({
  selector: "app-grosses-barchart",
  standalone: true,
  templateUrl: "./grosses-barchart.component.html",
  styleUrls: [],
  imports: [CommonModule, CardModule, SkeletonModule, ChartModule, RangeCalendarComponent],
})
export class GrossesBarchartComponent implements OnInit {
  public isLoading: boolean;
  public monthlyGrosses?: MonthlyGrossesDto;
  public chartData: unknown;
  public chartOptions: unknown;
  public startDate: Date;
  public endDate: Date;

  public readonly truckId = input<string>();
  @Input() chartColor: string;
  public readonly chartDrawn = output<BarChartDrawnEvent>();

  constructor(private apiService: ApiService) {
    this.isLoading = false;
    this.chartColor = "#EC407A";
    this.startDate = DateUtils.thisYear();
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
          label: "Monthly Gross",
          data: [],
        },
      ],
    };
  }

  ngOnInit(): void {
    this.fetchMonthlyGrosses();
  }

  fetchMonthlyGrosses() {
    this.isLoading = true;

    this.apiService
      .getMonthlyGrosses(this.startDate, this.endDate, this.truckId())
      .subscribe((result) => {
        if (result.success && result.data) {
          this.monthlyGrosses = result.data;
          const rpm =
            this.monthlyGrosses.totalGross /
            Converters.metersTo(this.monthlyGrosses.totalDistance, "mi");

          this.drawChart(this.monthlyGrosses);
          this.chartDrawn.emit({monthlyGrosses: this.monthlyGrosses, rpm: rpm});
        }

        this.isLoading = false;
      });
  }

  private drawChart(grosses: MonthlyGrossesDto) {
    const labels: string[] = [];
    const data: number[] = [];

    grosses.data.forEach((i) => {
      labels.push(DateUtils.monthNameWithYear(i.date));
      data.push(i.gross);
    });

    this.chartData = {
      labels: labels,
      datasets: [
        {
          label: "Monthly Gross",
          data: data,
          fill: true,
          tension: 0.4,
          backgroundColor: this.chartColor,
        },
      ],
    };
  }
}

export interface BarChartDrawnEvent {
  monthlyGrosses: MonthlyGrossesDto;
  rpm: number;
}
