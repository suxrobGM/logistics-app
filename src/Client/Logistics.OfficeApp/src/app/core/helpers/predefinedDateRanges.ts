import {DateRange} from './dateRange';


export abstract class PredefinedDateRanges {
  static getThisWeek(): DateRange {
    const start = new Date();
    start.setDate(start.getDate() - start.getDay() + 1);
    const end = new Date(start);
    end.setDate(start.getDate() + 6);
    return new DateRange('This Week', start, end);
  }

  static getLastWeek(): DateRange {
    const start = new Date();
    start.setDate(start.getDate() - start.getDay() - 6);
    const end = new Date(start);
    end.setDate(start.getDate() + 6);
    return new DateRange('Last Week', start, end);
  }

  static getPastTwoWeeks(): DateRange {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 14);
    return new DateRange('Past Two Weeks', start, end);
  }

  static getThisMonth(): DateRange {
    const start = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    const end = new Date();
    return new DateRange('This Month', start, end);
  }

  static getLastMonth(): DateRange {
    const start = new Date(new Date().getFullYear(), new Date().getMonth() - 1, 1);
    const end = new Date(new Date().getFullYear(), new Date().getMonth(), 0);
    return new DateRange('Last Month', start, end);
  }

  static getPast90Days(): DateRange {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 90);
    return new DateRange('Past 90 Days', start, end);
  }

  static getThisYear(): DateRange {
    const start = new Date(new Date().getFullYear(), 0, 1);
    const end = new Date();
    return new DateRange('This Year', start, end);
  }

  static getLastYear(): DateRange {
    const start = new Date(new Date().getFullYear() - 1, 0, 1);
    const end = new Date(new Date().getFullYear() - 1, 11, 31);
    return new DateRange('Last Year', start, end);
  }
}
