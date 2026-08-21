import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { Api, previewCounterOffer, type CounterOfferPreviewDto } from "@logistics/shared/api";
import { Spinner } from "@logistics/shared/ui";

/**
 * The counter-offer email as the broker will receive it, rendered from the pending decision so the
 * approver reads the real message rather than a summary of it.
 */
@Component({
  selector: "app-counter-offer-preview",
  templateUrl: "./counter-offer-preview.html",
  imports: [Spinner],
})
export class CounterOfferPreview implements OnInit {
  public readonly decisionId = input.required<string>();

  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly failed = signal(false);
  protected readonly preview = signal<CounterOfferPreviewDto | null>(null);

  /**
   * The server renders HTML, but binding it here would put an `[innerHTML]` on a page that shows
   * model-authored text. The words are what the approver needs, so the markup is stripped instead.
   */
  protected readonly bodyText = computed(() => toPlainText(this.preview()?.htmlBody ?? ""));

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

function toPlainText(html: string): string {
  return html
    .replace(/<(script|style)[\s\S]*?<\/\1>/gi, "")
    .replace(/<\/(p|div|tr|h[1-6])>/gi, "\n")
    .replace(/<br\s*\/?>/gi, "\n")
    .replace(/<[^>]*>/g, "")
    .replace(/&nbsp;/g, " ")
    .replace(/&amp;/g, "&")
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&#39;|&apos;/g, "'")
    .replace(/&quot;/g, '"')
    .replace(/[ \t]+/g, " ")
    .replace(/\n{3,}/g, "\n\n")
    .trim();
}
