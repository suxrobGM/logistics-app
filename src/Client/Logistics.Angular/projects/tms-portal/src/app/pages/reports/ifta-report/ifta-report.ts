import { DecimalPipe, SlicePipe } from "@angular/common";
import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { Api, exportIftaReportPdf, getIftaReport, type IftaReportDto } from "@logistics/shared/api";
import {
  Alert,
  Badge,
  DashboardCard,
  EmptyState,
  Spinner,
  Stack,
  UiButton,
  UiDataTable,
  UiSelectField,
  UiTooltip,
} from "@logistics/shared/ui";
import { downloadBlobFile } from "@logistics/shared/utils";
import { ToastService } from "@/core/services";
import { PageHeader, StatCard } from "@/shared/components";

interface QuarterOption {
  label: string;
  value: string; // "2026-3"
  year: number;
  quarter: number;
}

@Component({
  selector: "app-ifta-report",
  templateUrl: "./ifta-report.html",
  imports: [
    Alert,
    Badge,
    DashboardCard,
    DecimalPipe,
    EmptyState,
    PageHeader,
    SlicePipe,
    Spinner,
    Stack,
    StatCard,
    UiButton,
    UiDataTable,
    UiSelectField,
    UiTooltip,
  ],
})
export class IftaReportComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly exporting = signal(false);
  protected readonly report = signal<IftaReportDto | null>(null);

  protected readonly quarterOptions: QuarterOption[] = buildQuarterOptions();
  protected readonly selectedQuarter = signal<string>(this.quarterOptions[0].value);

  protected readonly hasData = computed(() => {
    const r = this.report();
    return !!r && ((r.jurisdictions?.length ?? 0) > 0 || r.totalMiles! > 0);
  });

  ngOnInit(): void {
    void this.fetch();
  }

  protected onQuarterChange(value: string | null): void {
    if (value) {
      this.selectedQuarter.set(value);
      void this.fetch();
    }
  }

  protected async fetch(): Promise<void> {
    const option = this.currentOption();
    this.isLoading.set(true);
    try {
      const data = await this.api.invoke(getIftaReport, {
        Year: option.year,
        Quarter: option.quarter,
      });
      this.report.set(data ?? null);
    } catch (error) {
      console.error("IFTA report fetch failed:", error);
      this.toast.showError("Failed to load the IFTA report. Please try again.");
      this.report.set(null);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async exportPdf(): Promise<void> {
    const option = this.currentOption();
    this.exporting.set(true);
    try {
      const blob = await this.api.invoke(exportIftaReportPdf, {
        Year: option.year,
        Quarter: option.quarter,
      });
      downloadBlobFile(blob, `ifta-report-${option.year}-q${option.quarter}.pdf`);
    } catch (error) {
      console.error("IFTA PDF export failed:", error);
      this.toast.showError("Failed to export the IFTA report PDF");
    } finally {
      this.exporting.set(false);
    }
  }

  protected exportCsv(): void {
    const report = this.report();
    const option = this.currentOption();
    if (!report?.jurisdictions?.length) {
      this.toast.showWarning("No IFTA data to export");
      return;
    }

    const headers = [
      "Jurisdiction",
      "Miles",
      "Taxable Gallons",
      "Purchased Gallons",
      "Net Taxable Gallons",
      "Rate Per Gallon",
      "Surcharge Per Gallon",
      "Tax Due",
    ];
    const rows = report.jurisdictions.map((row) => [
      `${row.countryCode}-${row.region}`,
      row.miles ?? "",
      row.taxableGallons ?? "",
      row.purchasedGallons ?? "",
      row.netTaxableGallons ?? "",
      row.rateMissing ? "missing" : (row.ratePerGallon ?? ""),
      row.surchargeRatePerGallon ?? "",
      row.taxDue ?? "",
    ]);

    const csvContent = [headers, ...rows]
      .map((row) => row.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(","))
      .join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    downloadBlobFile(blob, `ifta-report-${option.year}-q${option.quarter}.csv`);
    this.toast.showSuccess(`Exported ${report.jurisdictions.length} jurisdiction(s) to CSV`);
  }

  private currentOption(): QuarterOption {
    return (
      this.quarterOptions.find((o) => o.value === this.selectedQuarter()) ?? this.quarterOptions[0]
    );
  }
}

/** Current quarter first, then the 7 before it. */
function buildQuarterOptions(): QuarterOption[] {
  const options: QuarterOption[] = [];
  const now = new Date();
  let year = now.getUTCFullYear();
  let quarter = Math.floor(now.getUTCMonth() / 3) + 1;

  for (let i = 0; i < 8; i++) {
    options.push({
      label: `Q${quarter} ${year}${i === 0 ? " (current)" : ""}`,
      value: `${year}-${quarter}`,
      year,
      quarter,
    });
    quarter--;
    if (quarter === 0) {
      quarter = 4;
      year--;
    }
  }

  return options;
}
