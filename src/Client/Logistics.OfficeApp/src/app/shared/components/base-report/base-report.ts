import { DestroyRef, inject, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Observable, finalize } from "rxjs";
import { ApiService } from "@/core/api";
import { Result } from "@/core/api/models";
import { ToastService } from "@/core/services";
import { DateUtils } from "@/shared/utils";

export interface ReportQueryParams {
  startDate: Date;
  endDate?: Date;
  search?: string;
}

export abstract class BaseReportComponent<T> {
  private readonly destroyRef = inject(DestroyRef);
  protected readonly isLoading = signal(false);
  protected readonly data = signal<T | null>(null);
  protected readonly apiService = inject(ApiService);
  protected readonly toastService = inject(ToastService);
  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());
  protected search = signal<string>("");

  protected abstract query(params: ReportQueryParams): Observable<Result<T>>;
  protected abstract drawChart(result: T): void;

  protected fetch(params: ReportQueryParams): void {
    const { startDate = DateUtils.today(), endDate = DateUtils.today() } = params;
    this.isLoading.set(true);

    this.query({ startDate, endDate })
      .pipe(
        finalize(() => this.isLoading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((r) => {
        if (r.data) {
          this.data.set(r.data);
          this.drawChart(r.data);
        }
      });
  }
  filter(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate(), search: this.search() });
  }
}
