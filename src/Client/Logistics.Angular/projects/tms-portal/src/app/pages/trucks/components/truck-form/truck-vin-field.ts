import { Component, computed, inject, input, signal } from "@angular/core";
import { FormField, type FieldTree } from "@angular/forms/signals";
import { Api, decodeVin } from "@logistics/shared/api";
import { Alert, Badge, Stack, UiButton, UiFormField, UiTextField } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import type { TruckFormModel } from "./truck-form";

@Component({
  selector: "app-truck-vin-field",
  templateUrl: "./truck-vin-field.html",
  imports: [Alert, Badge, FormField, Stack, UiButton, UiFormField, UiTextField],
})
export class TruckVinField {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly field = input.required<FieldTree<TruckFormModel>>();

  protected readonly decoding = signal(false);
  protected readonly decodedSource = signal<string | null>(null);
  protected readonly decodedModelMissing = signal(false);

  protected readonly sourceLabel = computed(() => {
    switch (this.decodedSource()) {
      case "wmi":
        return "via WMI";
      case "nhtsa":
        return "via NHTSA";
      case "wmi+nhtsa":
        return "via WMI + NHTSA";
      default:
        return null;
    }
  });

  protected async decodeVin(): Promise<void> {
    const vin = this.field().vin().value()?.trim().toUpperCase() ?? "";

    if (vin.length !== 17) {
      this.toastService.showError("VIN must be exactly 17 characters");
      return;
    }

    this.decoding.set(true);

    try {
      const result = await this.api.invoke(decodeVin, { vin });

      if (result.make) this.field().make().value.set(result.make);
      if (result.model) this.field().model().value.set(result.model);
      if (result.year) this.field().year().value.set(result.year);

      this.decodedSource.set(result.source ?? null);
      this.decodedModelMissing.set(result.source === "wmi" && !result.model);
      this.toastService.showSuccess("VIN decoded successfully");
    } catch {
      this.decodedSource.set(null);
      this.decodedModelMissing.set(false);
      this.toastService.showError("Unable to decode VIN. Please verify the VIN is correct.");
    } finally {
      this.decoding.set(false);
    }
  }
}
