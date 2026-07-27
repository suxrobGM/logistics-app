import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { Api, getDailyGrosses, type DailyGrossesDto } from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { LocalizationService } from "@logistics/shared/services";
import { Card, Divider, Icon, Skeleton, UiChart } from "@logistics/shared/ui";
import { ThemeService } from "@/core/services";
import { getChartPalette, getLineGradient } from "@/shared/constants/chart-palette";
import { summarizeDailyGrosses, weeklyGrossStartDate } from "../../utils/weekly-gross";

@Component({
  selector: "app-daily-gross-chart",
  templateUrl: "./daily-gross-chart.html",
  imports: [Card, CurrencyFormatPipe, Divider, Icon, Skeleton, UiChart],
})
export class DailyGrossChartComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly localizationService = inject(LocalizationService, { optional: true });
  private readonly themeService = inject(ThemeService);

  public readonly chartClass = input<string>("");

  protected readonly isLoading = signal(false);
  protected readonly dailyGrosses = signal<DailyGrossesDto | null>(null);
  protected readonly chartData = signal<Record<string, unknown>>({ labels: [], datasets: [] });

  protected readonly summary = computed(() => summarizeDailyGrosses(this.dailyGrosses()));

  protected readonly chartOptions = computed(() => {
    const currencySymbol = this.localizationService?.getCurrencySymbol() ?? "$";
    const palette = getChartPalette(this.themeService.isDark());
    return {
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
            callback: (value: number) => `${currencySymbol}${value.toLocaleString()}`,
          },
          border: {
            display: false,
          },
        },
      },
    };
  });

  ngOnInit(): void {
    this.fetchDailyGrosses();
  }

  async fetchDailyGrosses(): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getDailyGrosses, {
      StartDate: weeklyGrossStartDate(),
    });

    if (result) {
      this.dailyGrosses.set(result);
      this.drawChart(result);
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

    const isDark = this.themeService.isDark();
    const palette = getChartPalette(isDark);

    this.chartData.set({
      labels,
      datasets: [
        {
          label: "Daily Gross",
          data,
          fill: true,
          tension: 0.4,
          borderColor: palette.primaryColor,
          borderWidth: 2,
          backgroundColor: (context: Parameters<typeof getLineGradient>[0]) =>
            getLineGradient(context, isDark),
          pointBackgroundColor: palette.primaryColor,
          pointBorderColor: "#fff",
          pointBorderWidth: 2,
          pointRadius: 4,
          pointHoverRadius: 6,
          pointHoverBackgroundColor: palette.primaryColor,
          pointHoverBorderColor: "#fff",
          pointHoverBorderWidth: 2,
        },
      ],
    });
  }
}
