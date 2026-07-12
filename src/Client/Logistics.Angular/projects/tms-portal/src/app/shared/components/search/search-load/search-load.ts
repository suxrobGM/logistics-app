import { Component, effect, ElementRef, inject, input, model, output, signal } from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl } from "@logistics/shared";
import { Api, getLoadById, getLoads, type LoadDto } from "@logistics/shared/api";
import { UiAutocompleteField } from "@logistics/shared/ui";
import { LoadStatusTag } from "../../tags/load-status-tag/load-status-tag";

/**
 * Component for searching and selecting loads.
 * It uses an autocomplete input to provide suggestions based on user input.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 */
@Component({
  selector: "app-search-load",
  templateUrl: "./search-load.html",
  imports: [LoadStatusTag, UiAutocompleteField],
})
export class SearchLoad implements FormValueControl<LoadDto | null> {
  private readonly api = inject(Api);

  protected readonly suggestedLoads = signal<LoadDto[]>([]);

  public readonly filterActiveLoads = input(false);

  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<LoadDto | null>(null);

  /** Driven by the Reactive Forms bridge / Signal Forms when present. */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  constructor() {
    // A parent form may write a bare load id (string) as the value instead of a
    // `LoadDto`. Normalise it by fetching the full load and writing it back.
    // (Selecting a load yields an object; clearing yields null — neither is a
    // string, so this only reacts to id-shaped writes and never loops.)
    effect(() => {
      const current = this.value() as LoadDto | string | null;
      if (typeof current === "string") {
        this.fetchLoad(current);
      }
    });
  }

  protected async searchLoad(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getLoads, {
      Search: event.query,
      OnlyActiveLoads: this.filterActiveLoads(),
    });

    if (!result.items) {
      return;
    }

    this.suggestedLoads.set(result.items);
  }

  private async fetchLoad(loadId: string): Promise<void> {
    if (!loadId) {
      this.value.set(null);
      return;
    }

    const result = await this.api.invoke(getLoadById, { id: loadId });
    if (result) {
      this.value.set(result);
    }
  }
}
