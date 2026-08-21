import { Component, DestroyRef, inject, signal, type OnInit } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { RouterLink } from "@angular/router";
import { PageHeader } from "@logistics/shared";
import { Api, getNegotiations, type RateNegotiationDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Card,
  EmptyState,
  Spinner,
  Stack,
  StatusBadge,
  UiButton,
  UiDataTable,
} from "@logistics/shared/ui";
import { AIDispatchHubService } from "@/core/services";
import { isOpenNegotiation } from "@/shared/utils";

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

  ngOnInit(): void {
    this.load();
    void this.hub.acquireDispatchBoard(this.destroyRef);

    // A broker reply, the expiry sweep and another dispatcher's close all land here, so rows are
    // replaced in place rather than re-fetching the whole page per event.
    this.hub.negotiationReceived$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((negotiation) => this.merge(negotiation));
  }

  protected toggleActiveOnly(): void {
    this.activeOnly.update((v) => !v);
    this.load();
  }

  protected lane(negotiation: RateNegotiationDto): string {
    const origin = this.place(negotiation.originCity, negotiation.originState);
    const destination = this.place(negotiation.destinationCity, negotiation.destinationState);
    return origin && destination ? `${origin} to ${destination}` : "-";
  }

  private place(city?: string | null, state?: string | null): string {
    return [city, state].filter(Boolean).join(", ");
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

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getNegotiations, {
        ActiveOnly: this.activeOnly(),
        PageSize: 50,
      });
      this.negotiations.set(result.items ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }
}
