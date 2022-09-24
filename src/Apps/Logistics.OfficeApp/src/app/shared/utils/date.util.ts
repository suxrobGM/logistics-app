/**
 * Date helper methods
 */
export class DateUtils {
  /**
   * Gets a today's date value.
   */
  today(): Date {
    return new Date();
  }

  /**
   * Gets this year with the specified month.
   * @param month Month from 0 to 11
   */
  thisYear(month = 0): Date {
    const today = this.today();
    const year = today.getFullYear();
    return new Date(year, month);
  }

  /**
   * Gets a day of the specified date.
   * @param dateStr Date value in string format
   */
  getDay(dateStr: string | Date): number {
    return new Date(dateStr).getDay();
  }

  /**
   * Converts a date string to a string by using the current locale.
   * @param date Specify date value either in string or the `Date` object.
   */
  toLocaleDate(date: string | Date): string {
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
  daysAgo(days: number): Date {
    const today = new Date();
    const daysAgo = new Date(today.setDate(today.getDate() - days));
    return daysAgo;
  }
}