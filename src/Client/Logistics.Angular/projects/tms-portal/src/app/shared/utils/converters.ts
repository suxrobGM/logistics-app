import type { AddressDto } from "@logistics/shared/api/models";

export abstract class Converters {
  /**
   * Converts an address object to the formatted string
   * @param value Address object
   * @returns A formatted string
   */
  static addressToString(value: AddressDto | null): string {
    if (!value) {
      return "";
    }
    if (value.line2) {
      return `${value.line1} ${value.line2}, ${value.city}, ${value.state} ${value.zipCode}`;
    }
    return `${value.line1}, ${value.city}, ${value.state} ${value.zipCode}`;
  }

  /**
   * Converts a distance value to the specified system of units.
   * The value must be in meters.
   * @param value Distance value in meter units.
   * @param unit Unit value, should be either `m`, `km`, `mi`, or `yd`.
   * @return Tranformed distance value in 2 decimals format.
   */
  static metersTo(meters: number, unit: DistanceUnitTypes): number {
    let convertedValue = meters;

    if (unit === "km") {
      convertedValue = meters * 0.001;
    } else if (unit === "mi") {
      convertedValue = meters * 0.000621371;
    } else if (unit === "yd") {
      convertedValue = meters * 1.09361;
    }

    return Number.parseFloat(convertedValue.toFixed(2));
  }

  /**
   * Converts a distance value to meters.
   * @param value Distance value
   * @param unit Unit of the distance value
   * @returns Distance value in meters
   */
  static toMeters(value: number, unit: DistanceUnitTypes): number {
    switch (unit) {
      case "km":
        return value / 0.001;
      case "mi":
        return value / 0.000621371;
      case "yd":
        return value / 1.09361;
      default:
        return value;
    }
  }

  /**
   * Converts a snake_case string to PascalCase.
   * @param value The snake_case string to convert
   * @returns The PascalCase string
   */
  static toPascalCase(value: string): string {
    return value
      .split("_")
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join("");
  }
}

export type DistanceUnitTypes = "m" | "km" | "mi" | "yd";
