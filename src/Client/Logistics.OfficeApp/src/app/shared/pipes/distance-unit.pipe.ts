import { Pipe, PipeTransform } from "@angular/core";
import { Converters, DistanceUnitTypes } from "@/shared/utils";

@Pipe({
  name: "distanceUnit",
})
export class DistanceUnitPipe implements PipeTransform {
  /**
   * Converts a distance value to the specified system of units.
   * The value must be in meters.
   * @param value Distance value in meter units.
   * @param unit Unit value, should be either `m`, `km`, `mi`, or `yd`.
   * @return Tranformed distance value in 2 decimals format.
   */
  transform(value?: number, unit: DistanceUnitTypes = "m"): number {
    if (!value) {
      return 0;
    }

    return Converters.metersTo(value, unit);
  }
}
