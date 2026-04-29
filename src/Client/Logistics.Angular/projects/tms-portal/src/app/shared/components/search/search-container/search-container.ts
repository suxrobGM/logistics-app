/* eslint-disable @typescript-eslint/no-empty-function */
import { Component, forwardRef, inject, model, output, signal } from "@angular/core";
import { FormsModule, NG_VALUE_ACCESSOR, type ControlValueAccessor } from "@angular/forms";
import { Api, getContainers, type ContainerDto } from "@logistics/shared/api";
import { AutoCompleteModule, type AutoCompleteSelectEvent } from "primeng/autocomplete";

@Component({
  selector: "app-search-container",
  templateUrl: "./search-container.html",
  imports: [AutoCompleteModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchContainer),
      multi: true,
    },
  ],
})
export class SearchContainer implements ControlValueAccessor {
  private readonly api = inject(Api);

  protected readonly suggestedContainers = signal<ContainerDto[]>([]);
  protected readonly lastQuery = signal<string>("");

  public readonly selectedContainer = model<ContainerDto | null>(null);
  public readonly selectedContainerChange = output<ContainerDto | null>();

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
    this.selectedContainerChange.emit(event.value);
    this.onChange(event.value);
  }

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  private onChange(value: ContainerDto | null): void {}
  private onTouched(): void {}

  writeValue(value: ContainerDto | null): void {
    this.selectedContainer.set(value);
  }

  registerOnChange(fn: () => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.selectedContainer.set(null);
    }
  }
}
