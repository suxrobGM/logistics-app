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

  getMonthName(month: number | Date): string {
    let monthValue = month;

    if (month instanceof Date) {
      monthValue = month.getMonth() + 1;
    }

    if (monthValue === 1) {
      return "January";
    }
    else if (monthValue === 2) {
      return "February";
    }
    else if (monthValue === 3) {
      return "March";
    }
    else if (monthValue === 4) {
      return "April";
    }
    else if (monthValue === 5) {
      return "May";
    }
    else if (monthValue === 6) {
      return "June";
    }
    else if (monthValue === 7) {
      return "July";
    }
    else if (monthValue === 8) {
      return "August";
    }
    else if (monthValue === 9) {
      return "September";
    }
    else if (monthValue === 10) {
      return "October";
    }
    else if (monthValue === 11) {
      return "November";
    }
    
    return "December";
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