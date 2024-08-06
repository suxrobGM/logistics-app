import {Pipe, PipeTransform} from '@angular/core';
import {AddressDto} from '@core/models';


@Pipe({
  name: 'address',
  standalone: true
})
export class AddressPipe implements PipeTransform {
  /**
   * Formats address without country name
   * @param value Address object
   * @returns A formatted string
   */
  transform(value?: AddressDto | null): string {
    if (!value) {
      return '';
    }
    if (value.line2) {
      return `${value.line1} ${value.line2}, ${value.city}, ${value.region} ${value.zipCode}`;
    }
    return `${value.line1}, ${value.city}, ${value.region} ${value.zipCode}`;
  }
}
