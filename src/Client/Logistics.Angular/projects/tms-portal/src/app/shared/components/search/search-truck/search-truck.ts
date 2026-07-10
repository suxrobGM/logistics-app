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
import { FormsModule } from "@angular/forms";
import type { FormValueControl } from "@angular/forms/signals";
import { focusFirstControl, isEmptyGuid } from "@logistics/shared";
import { Api, getTruckById, getTrucks, type TruckDto } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";

/**
 * Component for searching and selecting a truck.
 * This component uses an autocomplete input to allow users to search for trucks by name or number.
 * Its value is always a TruckDto; pass `[truckId]` to seed it from a bare ID.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `formControlName`, `[(ngModel)]` and `[formField]` alike — no
 * `ControlValueAccessor`, no compat shim.
 */
@Component({
  selector: "app-search-truck",
  templateUrl: "./search-truck.html",
  imports: [AutoCompleteModule, FormsModule],
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

  protected changeSelectedTruck(event: AutoCompleteSelectEvent): void {
    this.value.set(event.value);
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
