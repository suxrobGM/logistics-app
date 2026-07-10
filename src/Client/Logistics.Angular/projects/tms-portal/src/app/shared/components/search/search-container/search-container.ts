import { Component, inject, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl } from "@angular/forms/signals";
import { Api, getContainers, type ContainerDto } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";

/**
 * Container search autocomplete.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 */
@Component({
  selector: "app-search-container",
  templateUrl: "./search-container.html",
  imports: [AutoCompleteModule, FormsModule],
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

  protected changeSelectedContainer(event: AutoCompleteSelectEvent): void {
    this.value.set(event.value);
  }
}
