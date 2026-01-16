import { inject, signal } from "@angular/core";
import { Api } from "@logistics/shared/api";
import { ToastService } from "@/core/services";
import { DateUtils } from "@/shared/utils";

export interface ReportQueryParams {
  startDate: Date;
  endDate?: Date;
  search?: string;
}

export abstract class BaseReportComponent<T> {
  protected readonly isLoading = signal(false);
  protected readonly data = signal<T | null>(null);
  protected readonly api = inject(Api);
  protected readonly toastService = inject(ToastService);
  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());
  protected search = signal<string>("");

  protected abstract query(params: ReportQueryParams): Promise<T>;
  protected abstract drawChart(result: T): void;

  protected async fetch(params: ReportQueryParams): Promise<void> {
    const { startDate = DateUtils.today(), endDate = DateUtils.today() } = params;
    this.isLoading.set(true);

    try {
      const result = await this.query({ startDate, endDate });
      if (result) {
        this.data.set(result);
        this.drawChart(result);
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  filter(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate(), search: this.search() });
  }
}
