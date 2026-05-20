import { Component, computed, inject, input, signal } from "@angular/core";
import { FormGroup, ReactiveFormsModule } from "@angular/forms";
import { Api, decodeVin } from "@logistics/shared/api";
import { FormField, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { MessageModule } from "primeng/message";
import { TagModule } from "primeng/tag";
import { ToastService } from "@/core/services";

@Component({
  selector: "app-truck-vin-field",
  templateUrl: "./truck-vin-field.html",
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    MessageModule,
    TagModule,
    FormField,
    Stack,
  ],
})
export class TruckVinField {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly form = input.required<FormGroup>();

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
    const vin = this.form().get("vin")?.value?.trim().toUpperCase() ?? "";

    if (vin.length !== 17) {
      this.toastService.showError("VIN must be exactly 17 characters");
      return;
    }

    this.decoding.set(true);

    try {
      const result = await this.api.invoke(decodeVin, { vin });

      const patch: Record<string, unknown> = {};
      if (result.make) patch["make"] = result.make;
      if (result.model) patch["model"] = result.model;
      if (result.year) patch["year"] = result.year;

      this.form().patchValue(patch);

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
