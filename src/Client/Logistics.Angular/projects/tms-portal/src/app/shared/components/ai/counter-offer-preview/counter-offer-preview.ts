import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { Api, previewCounterOffer, type CounterOfferPreviewDto } from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Spinner } from "@logistics/shared/ui";

/**
 * The counter-offer email as the broker will receive it, rendered from the pending decision so the
 * approver reads the real message rather than a summary of it.
 */
@Component({
  selector: "app-counter-offer-preview",
  templateUrl: "./counter-offer-preview.html",
  imports: [CurrencyFormatPipe, Spinner],
})
export class CounterOfferPreview implements OnInit {
  public readonly decisionId = input.required<string>();

  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly failed = signal(false);
  protected readonly preview = signal<CounterOfferPreviewDto | null>(null);

  async ngOnInit(): Promise<void> {
    this.isLoading.set(true);
    try {
      this.preview.set(
        await this.api.invoke(previewCounterOffer, { decisionId: this.decisionId() }),
      );
    } catch {
      this.failed.set(true);
    } finally {
      this.isLoading.set(false);
    }
  }
}
