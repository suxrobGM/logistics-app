import { Component, effect, ElementRef, inject, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl } from "@logistics/shared";
import { Api, getLoadById, getLoads, type LoadDto } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";
import { LoadStatusTag } from "../../tags/load-status-tag/load-status-tag";

/**
 * Component for searching and selecting loads.
 * It uses the AutoComplete component from PrimeNG to provide suggestions based on user input.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 */
@Component({
  selector: "app-search-load",
  templateUrl: "./search-load.html",
  imports: [AutoCompleteModule, FormsModule, LoadStatusTag],
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

  protected changeSelectedLoad(event: AutoCompleteSelectEvent): void {
    this.value.set(event.value);
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
