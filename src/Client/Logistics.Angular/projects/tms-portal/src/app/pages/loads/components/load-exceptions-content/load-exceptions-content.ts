import { CommonModule, DatePipe } from "@angular/common";
import { Component, inject, input, output, signal, type OnInit } from "@angular/core";
import { Api, getLoadExceptions, type LoadExceptionDto } from "@logistics/shared/api";
import {
  Card,
  CountBadge,
  Icon,
  Spinner,
  UiButton,
  UiDataTable,
  UiTooltip,
} from "@logistics/shared/ui";
import { ExceptionTypeTag } from "@/shared/components/tags";

@Component({
  selector: "app-load-exceptions-content",
  templateUrl: "./load-exceptions-content.html",
  imports: [
    Card,
    CommonModule,
    CountBadge,
    DatePipe,
    ExceptionTypeTag,
    Icon,
    Spinner,
    UiButton,
    UiDataTable,
    UiTooltip,
  ],
})
export class LoadExceptionsContent implements OnInit {
  private readonly api = inject(Api);

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
      const result = await this.api.invoke(getLoadExceptions, { id: loadId });
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
