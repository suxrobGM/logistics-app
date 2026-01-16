import { AbstractControl, type ValidationErrors } from "@angular/forms";

export abstract class PaymentMethodValidators {
  /**
   * Validates the card expiration date in the format MM/YY.
   * @param control The form control to validate.
   * @returns An object with validation errors or null if valid.
   */
  static cardExpDate(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value || value.length < 5) return null;

    const [monthStr, yearStr] = value.split("/");
    const month = parseInt(monthStr, 10);
    const year = parseInt(yearStr, 10);

    // Validate month
    if (month < 1 || month > 12) {
      return { invalidMonth: true };
    }

    const currentYear = new Date().getFullYear();
    const currentMonth = new Date().getMonth() + 1;

    // Validate expiration
    if (year < currentYear || (year === currentYear && month < currentMonth)) {
      return { expired: true };
    }

    return null;
  }
}
