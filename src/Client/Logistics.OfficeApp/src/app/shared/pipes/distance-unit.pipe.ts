import {Pipe, PipeTransform} from '@angular/core';

@Pipe({name: 'distanceUnit'})
export class DistanceUnitPipe implements PipeTransform {
  /**
   * Converts a distance value to the specified system of units.
   * The value must be in meters.
   * @param value Distance value in meter units.
   * @param unit Unit value, should be either `m`, `km`, `mi`, or `yd`.
   * @return Tranformed distance value in 2 decimals format.
   */
  transform(value?: number, unit: DistanceUnitTypes = 'm'): number {
    if (!value) {
      return 0;
    }

    let convertedValue = value;

    if (unit === 'km') {
      convertedValue = value*0.001;
    }
    else if (unit === 'mi') {
      convertedValue = value*0.000621371;
    }
    else if (unit === 'yd') {
      convertedValue = value*1.09361;
    }

    return Number.parseFloat(convertedValue.toFixed(2));
  }
}

type DistanceUnitTypes = 'm' | 'km' | 'mi' | 'yd';
