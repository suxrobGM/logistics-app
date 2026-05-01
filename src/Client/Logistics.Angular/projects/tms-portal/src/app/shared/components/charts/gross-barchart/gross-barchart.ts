import { Component, computed, inject, input, output, signal, type OnInit } from "@angular/core";
import { Api, getMonthlyGrosses, type MonthlyGrossesDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { LocalizationService } from "@logistics/shared/services";
import { adjustColorBrightness } from "@logistics/shared/utils";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { ThemeService } from "@/core/services";
import { DateRangePicker } from "@/shared/components/other";
import { getChartPalette } from "@/shared/constants/chart-palette";
import { Converters, DateUtils } from "@/shared/utils";

@Component({
  selector: "app-gross-barchart",
  templateUrl: "./gross-barchart.html",
  imports: [
    CardModule,
    SkeletonModule,
    ChartModule,
    DateRangePicker,
    DividerModule,
    CurrencyFormatPipe,
    DistanceUnitPipe,
  ],
})
export class GrossBarchart implements OnInit {
  private readonly api = inject(Api);
  private readonly localizationService = inject(LocalizationService, { optional: true });
  private readonly themeService = inject(ThemeService);

  protected readonly isLoading = signal(false);
  protected readonly monthlyGrosses = signal<MonthlyGrossesDto | null>(null);
  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());
  protected readonly chartData = signal<Record<string, unknown>>({ labels: [], datasets: [] });

  public readonly truckId = input<string>();
  public readonly chartColor = input<string | null>(null);
  public readonly chartDrawn = output<BarChartDrawnEvent>();

  protected readonly totalGross = computed(() => this.monthlyGrosses()?.totalGross ?? 0);
  protected readonly totalDistance = computed(() => {
    const unit = this.localizationService?.getDistanceUnit() ?? "mi";
    return Converters.metersTo(this.monthlyGrosses()?.totalDistance ?? 0, unit);
  });
  protected readonly rpm = computed(() => {
    const distance = this.totalDistance();
    return distance > 0 ? this.totalGross() / distance : 0;
  });

  protected readonly chartOptions = computed(() => {
    const currencySymbol = this.localizationService?.getCurrencySymbol() ?? "$";
    const palette = getChartPalette(this.themeService.isDark());
    return {
      maintainAspectRatio: false,
      responsive: true,
      plugins: {
        legend: {
          display: false,
        },
        tooltip: {
          backgroundColor: palette.tooltipBg,
          titleColor: palette.titleColor,
          bodyColor: palette.textColor,
          borderColor: palette.tooltipBorder,
          borderWidth: 1,
          titleFont: { size: 13, weight: "600" },
          bodyFont: { size: 12 },
          padding: 12,
          cornerRadius: 8,
          displayColors: false,
          callbacks: {
            label: (context: { parsed: { y: number } }) => {
              const value = context.parsed.y;
              return ` ${currencySymbol}${value.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
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
            color: palette.textColor,
          },
          border: {
            display: false,
          },
        },
        y: {
          beginAtZero: true,
          grid: {
            color: palette.gridColor,
          },
          ticks: {
            font: { size: 11 },
            color: palette.textColor,
            callback: (value: number) => `${currencySymbol}${(value / 1000).toFixed(0)}k`,
          },
          border: {
            display: false,
          },
        },
      },
    };
  });

  ngOnInit(): void {
    this.fetchMonthlyGrosses();
  }

  onDateRangeChange(dates: Date[]): void {
    if (dates.length === 2) {
      this.startDate.set(dates[0]);
      this.endDate.set(dates[1]);
      this.fetchMonthlyGrosses();
    }
  }

  async fetchMonthlyGrosses(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getMonthlyGrosses, {
      StartDate: this.startDate().toISOString(),
      EndDate: this.endDate().toISOString(),
      TruckId: this.truckId(),
    });

    if (result) {
      this.monthlyGrosses.set(result);
      this.drawChart(result);
      this.chartDrawn.emit({ monthlyGrosses: result, rpm: this.rpm() });
    }

    this.isLoading.set(false);
  }

  private drawChart(grosses: MonthlyGrossesDto): void {
    const labels: string[] = [];
    const data: number[] = [];

    grosses.data?.forEach((i) => {
      labels.push(DateUtils.monthNameWithYear(i.date!));
      data.push(i.gross ?? 0);
    });

    const isDark = this.themeService.isDark();
    const palette = getChartPalette(isDark);
    const color = this.chartColor() ?? palette.primaryColor;

    this.chartData.set({
      labels,
      datasets: [
        {
          label: "Monthly Gross",
          data,
          backgroundColor: color,
          hoverBackgroundColor: adjustColorBrightness(color, -15),
          borderRadius: 6,
          borderSkipped: false,
          maxBarThickness: 48,
        },
      ],
    });
  }
}

export interface BarChartDrawnEvent {
  monthlyGrosses: MonthlyGrossesDto;
  rpm: number;
}
