import { Component, effect, inject, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl } from "@angular/forms/signals";
import { isEmptyGuid } from "@logistics/shared";
import { Api, getTruckById, getTrucks, type TruckDto } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";

/**
 * Component for searching and selecting a truck.
 * This component uses an autocomplete input to allow users to search for trucks by name or number.
 * It accepts a truck ID or a TruckDto object as input and emits the selected truck.
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

  /** Driven by the Reactive Forms bridge / consumers to disable the input. */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  constructor() {
    // Reactive-form controls can be seeded with a bare truck ID (string) instead of a TruckDto
    // (see load-form). Resolve that ID to the full object. Only fires when the value is a string,
    // so writing the resolved TruckDto back below cannot re-trigger a fetch (no write-back loop).
    effect(() => {
      const current = this.value() as TruckDto | string | null;
      if (typeof current === "string") {
        this.resolveTruckById(current);
      }
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
    if (isEmptyGuid(id)) {
      this.value.set(null);
      return;
    }

    const result = await this.api.invoke(getTruckById, { truckOrDriverId: id });
    if (result) {
      this.value.set(result);
    }
  }
}
