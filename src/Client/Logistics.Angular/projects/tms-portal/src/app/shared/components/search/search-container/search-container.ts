import { Component, ElementRef, inject, input, model, output, signal } from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl } from "@logistics/shared";
import { Api, getContainers, type ContainerDto } from "@logistics/shared/api";
import { UiAutocompleteField } from "@logistics/shared/ui";

/**
 * Container search autocomplete.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 */
@Component({
  selector: "app-search-container",
  templateUrl: "./search-container.html",
  imports: [UiAutocompleteField],
})
export class SearchContainer implements FormValueControl<ContainerDto | null> {
  private readonly api = inject(Api);

  protected readonly suggestedContainers = signal<ContainerDto[]>([]);
  protected readonly lastQuery = signal<string>("");

  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<ContainerDto | null>(null);

  /** The Reactive Forms bridge drives this; Signal Forms binds it when present. */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  protected async searchContainer(event: { query: string }): Promise<void> {
    const q = event.query?.trim() ?? "";
    this.lastQuery.set(q);

    if (q.length < 2) {
      this.suggestedContainers.set([]);
      return;
    }

    try {
      const result = await this.api.invoke(getContainers, { Search: q });
      this.suggestedContainers.set(result.items ?? []);
    } catch {
      this.suggestedContainers.set([]);
    }
  }
}
