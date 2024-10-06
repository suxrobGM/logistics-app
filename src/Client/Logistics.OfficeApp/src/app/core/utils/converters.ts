import {AddressDto} from '@/core/models';

export abstract class Converters {
  /**
   * Converts an address object to the formatted string
   * @param value Address object
   * @returns A formatted string
   */
  static addressToString(value: AddressDto | null): string {
    if (!value) {
      return '';
    }
    if (value.line2) {
      return `${value.line1} ${value.line2}, ${value.city}, ${value.region} ${value.zipCode}`;
    }
    return `${value.line1}, ${value.city}, ${value.region} ${value.zipCode}`;
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

    if (unit === 'km') {
      convertedValue = meters*0.001;
    }
    else if (unit === 'mi') {
      convertedValue = meters*0.000621371;
    }
    else if (unit === 'yd') {
      convertedValue = meters*1.09361;
    }

    return Number.parseFloat(convertedValue.toFixed(2));
  }
}

export type DistanceUnitTypes = 'm' | 'km' | 'mi' | 'yd';
