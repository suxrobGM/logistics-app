import { Component, DestroyRef, inject, signal, type OnInit } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { RouterLink } from "@angular/router";
import { getApiErrorMessage, PageHeader } from "@logistics/shared";
import { Api, getNegotiations, type RateNegotiationDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Card,
  EmptyState,
  ErrorState,
  Spinner,
  Stack,
  StatusBadge,
  UiButton,
  UiDataTable,
} from "@logistics/shared/ui";
import { AIDispatchHubService } from "@/core/services";
import { formatNegotiationLane, isOpenNegotiation } from "@/shared/utils";

@Component({
  selector: "app-negotiations",
  templateUrl: "./negotiations.html",
  imports: [
    Card,
    CurrencyFormatPipe,
    DateFormatPipe,
    PageHeader,
    RouterLink,
    Spinner,
    Stack,
    UiButton,
    UiDataTable,
    EmptyState,
    ErrorState,
    StatusBadge,
  ],
})
export class Negotiations implements OnInit {
  private readonly api = inject(Api);
  private readonly hub = inject(AIDispatchHubService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly isLoading = signal(false);
  protected readonly negotiations = signal<RateNegotiationDto[]>([]);
  protected readonly activeOnly = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    void this.load();
    void this.hub.connect(this.destroyRef);

    // A broker reply, the expiry sweep and another dispatcher's close all land here, so rows are
    // replaced in place rather than re-fetching the whole page per event.
    this.hub.negotiationReceived$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((negotiation) => this.merge(negotiation));
  }

  protected toggleActiveOnly(): void {
    this.activeOnly.update((v) => !v);
    void this.load();
  }

  protected lane(negotiation: RateNegotiationDto): string {
    return formatNegotiationLane(negotiation);
  }

  protected async load(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    try {
      const result = await this.api.invoke(getNegotiations, {
        ActiveOnly: this.activeOnly(),
        PageSize: 50,
      });
      this.negotiations.set(result.items ?? []);
    } catch (error) {
      this.error.set(getApiErrorMessage(error, "Failed to load the negotiations"));
    } finally {
      this.isLoading.set(false);
    }
  }

  private merge(updated: RateNegotiationDto): void {
    this.negotiations.update((rows) => {
      const index = rows.findIndex((r) => r.id === updated.id);

      if (index < 0) {
        return this.activeOnly() && !isOpenNegotiation(updated.status) ? rows : [updated, ...rows];
      }

      if (this.activeOnly() && !isOpenNegotiation(updated.status)) {
        return rows.filter((r) => r.id !== updated.id);
      }

      const next = [...rows];
      next[index] = updated;
      return next;
    });
  }
}
