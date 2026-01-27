import { CommonModule, DatePipe } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Component, type OnInit, inject, input, output, signal } from "@angular/core";
import { ApiConfiguration } from "@logistics/shared/api";
import type { LoadExceptionDto } from "@logistics/shared/api";
import { BadgeModule } from "primeng/badge";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { firstValueFrom } from "rxjs";
import { ExceptionTypeTag } from "@/shared/components/tags";

@Component({
  selector: "app-load-exceptions-content",
  templateUrl: "./load-exceptions-content.html",
  imports: [
    CommonModule,
    CardModule,
    TableModule,
    DatePipe,
    ButtonModule,
    TooltipModule,
    ProgressSpinnerModule,
    ExceptionTypeTag,
    BadgeModule,
  ],
})
export class LoadExceptionsContent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  public readonly loadId = input.required<string>();
  public readonly reportException = output<void>();
  public readonly resolveException = output<LoadExceptionDto>();

  protected readonly isLoading = signal(false);
  protected readonly exceptions = signal<LoadExceptionDto[]>([]);

  protected readonly unresolvedCount = signal(0);

  ngOnInit(): void {
    this.loadExceptions(this.loadId());
  }

  refresh(): void {
    const id = this.loadId();
    if (id) {
      this.loadExceptions(id);
    }
  }

  protected onReportException(): void {
    this.reportException.emit();
  }

  protected onResolveException(exception: LoadExceptionDto): void {
    this.resolveException.emit(exception);
  }

  protected isResolved(exception: LoadExceptionDto): boolean {
    return !!exception.resolvedAt;
  }

  private async loadExceptions(loadId: string): Promise<void> {
    this.isLoading.set(true);

    try {
      const url = `${this.apiConfig.rootUrl}/loads/${loadId}/exceptions`;
      const result = await firstValueFrom(this.http.get<LoadExceptionDto[]>(url));
      this.exceptions.set(result ?? []);
      this.unresolvedCount.set(result?.filter((e) => !e.resolvedAt).length ?? 0);
    } catch {
      this.exceptions.set([]);
      this.unresolvedCount.set(0);
    } finally {
      this.isLoading.set(false);
    }
  }
}
