/**
 * Date helper methods
 */
export abstract class DateUtils {
  /**
   * Gets a today's date value.
   */
  static today(): Date {
    return new Date();
  }

  /**
   * Gets this year with the specified month.
   * @param month Month from 0 to 11
   */
  static thisYear(month = 0): Date {
    const today = this.today();
    const year = today.getFullYear();
    return new Date(year, month);
  }

  /**
   * Gets the day-of-the-month.
   * @param dateStr Date value in string format
   */
  static dayOfMonth(dateStr: string | Date): number {
    return new Date(dateStr).getDate();
  }

  /**
   * Gets month name and year in the format `{monthName} - {year}`
   * @param dateStr A string containing valid date
   */
  static monthNameWithYear(dateStr: string): string {
    const date = new Date(dateStr);
    const monthName = this.monthName(date);
    return `${monthName} ${date.getFullYear()}`;
  }

  /**
   * Gets months name from index
   * @param month Month index or date type
   */
  static monthName(month: number | Date): string {
    let monthValue = month;

    if (month instanceof Date) {
      monthValue = month.getMonth() + 1;
    }

    if (monthValue === 1) {
      return 'January';
    }
    else if (monthValue === 2) {
      return 'February';
    }
    else if (monthValue === 3) {
      return 'March';
    }
    else if (monthValue === 4) {
      return 'April';
    }
    else if (monthValue === 5) {
      return 'May';
    }
    else if (monthValue === 6) {
      return 'June';
    }
    else if (monthValue === 7) {
      return 'July';
    }
    else if (monthValue === 8) {
      return 'August';
    }
    else if (monthValue === 9) {
      return 'September';
    }
    else if (monthValue === 10) {
      return 'October';
    }
    else if (monthValue === 11) {
      return 'November';
    }

    return 'December';
  }

  /**
   * Converts a date string to a string by using the current locale.
   * @param date Specify date value either in string or the `Date` object.
   */
  static toLocaleDate(date: string | Date): string {
    if (date instanceof Date) {
      return date.toLocaleDateString();
    }

    return new Date(date).toLocaleDateString();
  }

  /**
   * Gets how many days have passed since today
   * @param days A desired number of days to get past date.
   * @returns A new `Date` object of the past date.
   */
  static daysAgo(days: number): Date {
    const today = new Date();
    const daysAgo = new Date(today.setDate(today.getDate() - days));
    return daysAgo;
  }

  /**
   * Checks whether two dates are valid range
   * @param dates An array of two dates
   */
  static isValidRange(dates: Date[]): boolean {
    const areDatesNotNull = dates && dates.length >= 2 && dates[0] != null && dates[1] != null;
    const areDatesValid = this.isValidDate(dates[0]) && this.isValidDate(dates[1]);
    const isEndDateGreater = dates[1] > dates[0];
    return areDatesNotNull && areDatesValid && isEndDateGreater;
  }

  /**
   * Checks whether the specified is instance of the Date class
   * @param obj Any object
   */
  static isValidDate(obj: unknown): boolean {
    if (obj instanceof Date) {
      return true;
    }

    return false;
  }
}
