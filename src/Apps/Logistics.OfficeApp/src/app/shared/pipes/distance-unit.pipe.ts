import { Pipe, PipeTransform } from '@angular/core';

@Pipe({name: 'distanceUnit'})
export class DistanceUnitPipe implements PipeTransform {
  transform(value: number, unit: string): number {
    return 0;
  }
}