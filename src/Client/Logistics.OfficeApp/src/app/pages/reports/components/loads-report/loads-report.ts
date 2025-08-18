import { Component, OnInit, signal } from '@angular/core';
import { Card } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { LoadsReportDto } from '@/core/api/models/report/loads-report.dto';
import { RangeCalendar } from '@/shared/components';
import { Observable } from 'rxjs';
import { Result } from '@/core/api/models';
import { BaseReportComponent, ReportQueryParams } from '@/shared/components/base-report/base-report';
import { ProgressSpinner } from 'primeng/progressspinner';

/* =========================
 * Constants (نفس الملف)
 * ========================= */
const CHART_PALETTE = ['#2563eb', '#16a34a', '#f59e0b', '#a855f7', '#ef4444', '#06b6d4', '#f97316'];

const CHART_INITIAL_DATA: any = {
  labels: ['No Data'],
  datasets: [
    {
      label: 'Loads by Status',
      data: [1],
      backgroundColor: ['#e5e5e5']
    }
  ]
};

function getCssVar(name: string) {
  return getComputedStyle(document.documentElement).getPropertyValue(name) || undefined;
}

function buildPieOptions() {
  const textColor = getCssVar('--text-color') || '#334155';

  return {
    responsive: true,
    maintainAspectRatio: false,     
    aspectRatio: 1,                 
    plugins: {
      legend: {
        position: 'bottom',
        labels: { color: textColor, usePointStyle: true, boxWidth: 8 }
      },
      tooltip: {
        callbacks: {
          label: (ctx: any) => `${ctx.label}: ${ctx.parsed}`
        }
      }
    },
    layout: { padding: 0 }
  };
}

function buildBarOptions() {
  const textColor = getCssVar('--text-color') || '#334155';
  const gridColor = '#495057';

  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: { color: textColor }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { color: textColor },
        grid: { color: gridColor }
      },
      x: {
        ticks: { color: textColor },
        grid: { color: gridColor }
      }
    }
  };
}

@Component({
  selector: 'app-loads-report',
  templateUrl: './loads-report.html',
  imports: [Card, ChartModule, TableModule, CurrencyPipe, RangeCalendar, DecimalPipe],
  standalone: true
})
export class LoadsReportComponent extends BaseReportComponent<LoadsReportDto> implements OnInit {
  // signals
  protected readonly chartData = signal<any>(CHART_INITIAL_DATA);

  // charts state
  protected pieOptions: any = buildPieOptions();
  protected typeChartData: any;
  protected typeChartOptions: any;

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override query(params: ReportQueryParams): Observable<Result<LoadsReportDto>> {
    return this.apiService.reportApi.getLoadsReport({
      startDate: params.startDate,
      endDate: params.endDate
    });
  }

  protected override drawChart(result: LoadsReportDto): void {
    // ----- Pie (Loads by Status) -----
    const status = result.statusBreakdown ?? [];
    if (!status.length) {
      this.chartData.set(CHART_INITIAL_DATA);
    } else {
      const labels = status.map(s => s.status);
      const data = status.map(s => s.count);
      const colors = CHART_PALETTE.slice(0, Math.max(1, labels.length));

      this.chartData.set({
        labels,
        datasets: [
          {
            label: 'Loads by Status',
            data,
            backgroundColor: colors
          }
        ]
      });
    }

    this.pieOptions = buildPieOptions();

    // ----- Bar (Loads by Type) -----
    const types = result.typeBreakdown ?? [];
    
    console.log(types);
    
    this.typeChartData = {
      labels: types.map(t => t.type),
      datasets: [
        {
          label: 'Revenue',
          data: types.map(t => t.totalRevenue),
          backgroundColor: '#42A5F5'
        }
      ]
    };
    this.typeChartOptions = buildBarOptions();
  }
}
