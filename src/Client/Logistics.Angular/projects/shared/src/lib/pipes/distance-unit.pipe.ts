import { Pipe, type PipeTransform, inject } from "@angular/core";
import { LocalizationService } from "../services/localization.service";
import { Converters, type DistanceUnitTypes } from "../utils/converters";

@Pipe({ name: "distanceUnit", standalone: true })
export class DistanceUnitPipe implements PipeTransform {
  private readonly localizationService = inject(LocalizationService, { optional: true });

  /**
   * Converts a distance value to the tenant's preferred unit and formats with label.
   * The value must be in meters.
   * @param value Distance value in meter units.
   * @param unit Optional override unit. If not provided, uses tenant settings.
   * @return Formatted distance string with unit label (e.g., "150.5 mi" or "242.3 km").
   */
  transform(value?: number | null, unit?: DistanceUnitTypes): string {
    if (value == null || value === 0) {
      const targetUnit = unit ?? this.localizationService?.getDistanceUnit() ?? "mi";
      return `0 ${targetUnit}`;
    }

    const targetUnit = unit ?? this.localizationService?.getDistanceUnit() ?? "mi";
    const converted = Converters.metersTo(value, targetUnit);
    return `${converted.toLocaleString("en-US", { maximumFractionDigits: 1 })} ${targetUnit}`;
  }
}
