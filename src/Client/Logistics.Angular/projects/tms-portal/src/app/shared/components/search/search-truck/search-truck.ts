import {
  Component,
  effect,
  ElementRef,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
} from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl, isEmptyGuid } from "@logistics/shared";
import { Api, getTruckById, getTrucks, type TruckDto } from "@logistics/shared/api";
import { UiAutocompleteField } from "@logistics/shared/ui";

/**
 * Component for searching and selecting a truck.
 * This component uses an autocomplete input to allow users to search for trucks by name or number.
 * Its value is always a TruckDto; pass `[truckId]` to seed it from a bare ID.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 */
@Component({
  selector: "app-search-truck",
  templateUrl: "./search-truck.html",
  imports: [UiAutocompleteField],
})
export class SearchTruck implements FormValueControl<TruckDto | null> {
  private readonly api = inject(Api);

  protected readonly suggestedTrucks = signal<TruckDto[]>([]);

  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<TruckDto | null>(null);

  /**
   * Seeds the control from a bare truck ID, which is all an edit form usually has. Resolved to the
   * full DTO and written into `value`.
   *
   * This is a separate input rather than a `string` member of `value` because `[formField]` value
   * types are invariant: widening `value` to `TruckDto | string | null` would force every consumer's
   * model field to that same type.
   */
  public readonly truckId = input<string | null>(null);

  /** Driven by the Reactive Forms bridge / consumers to disable the input. */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  constructor() {
    // Resolve the seeded ID to the full DTO. Reading `value` untracked keeps this from re-running
    // when the resolved truck is written back, and the id-equality check keeps a later user
    // selection (or a clear) from being undone.
    effect(() => {
      const id = this.truckId();
      if (!id || isEmptyGuid(id)) {
        return;
      }
      if (untracked(this.value)?.id === id) {
        return;
      }
      this.resolveTruckById(id);
    });
  }

  protected async searchTruck(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getTrucks, { Search: event.query });
    this.suggestedTrucks.set(result.items ?? []);
  }

  /** Marks the control as touched so validation errors surface (on blur). */
  protected markTouched(): void {
    this.touch.emit();
  }

  private async resolveTruckById(id: string): Promise<void> {
    const result = await this.api.invoke(getTruckById, { truckOrDriverId: id });
    if (result) {
      this.value.set(result);
    }
  }
}
