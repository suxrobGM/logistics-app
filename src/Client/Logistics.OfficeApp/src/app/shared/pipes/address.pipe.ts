import {Pipe, PipeTransform} from "@angular/core";
import {AddressDto} from "@/core/api/models";

@Pipe({
  name: "address",
})
export class AddressPipe implements PipeTransform {
  /**
   * Formats address without country name
   * @param value Address object
   * @returns A formatted string
   */
  transform(value?: AddressDto | null): string {
    if (this.isNullAddress(value)) {
      return "";
    }
    if (value.line2) {
      return `${value.line1} ${value.line2}, ${value.city}, ${value.state} ${value.zipCode}`;
    }
    return `${value.line1}, ${value.city}, ${value.state} ${value.zipCode}`;
  }

  /**
   * Checks if the address is null or contains invalid values
   * @param value Address object
   * @returns true if the address is null or contains invalid values, false otherwise
   */
  private isNullAddress(value?: AddressDto | null): value is null | undefined {
    return (
      value == null ||
      value.line1 == null ||
      value.line1 === "NULL" ||
      value.city == null ||
      value.city === "NULL" ||
      value.state == null ||
      value.state === "NULL" ||
      value.zipCode == null ||
      value.zipCode === "NULL" ||
      value.country == null ||
      value.country === "NULL"
    );
  }
}
